# Dashboards, alertas e healthcheck — New Relic

Dashboards, condições de alerta e o synthetic monitor são **código Terraform** no repositório
`infra-k8s`, stack `terraform/observability` (workspace HCP `wrench_auto_repair_observability`),
aplicado pelo pipeline a cada push em `master`. Este documento descreve o que cada painel mostra, de
onde vem o dado e as consultas NRQL usadas, para leitura e para exploração ad hoc no New Relic.

Todas as consultas filtram `service.name = 'wrench-auto-repair-api'` (definido em
`Observability:ServiceName`, ver [ADR 003](../adrs/ADR%20003%20-%20Observabilidade%20com%20New%20Relic.md))
e `clusterName = 'eks-wrench-auto-repair'`. Os dois valores são variáveis do stack
(`service_name` e `cluster_name`).

As rotas usam `SlugifyParameterTransformer`, então `OrdemServicoController` aparece como
`api/v1/ordem-servico` em `http.route`.

A instrumentação OpenTelemetry da API (1.18) segue a convenção semântica estável: `http.request.method`
e `http.response.status_code` nos spans de servidor, `server.address` e `url.full` nos spans de
cliente. As consultas usam apenas esses atributos, sem `coalesce`, que as condições de alerta NRQL não
aceitam. Para inspecionar os atributos disponíveis:

```sql
SELECT keyset() FROM Span WHERE service.name = 'wrench-auto-repair-api' SINCE 1 hour ago
```

---

## Dashboard `Wrench Auto Repair`

### Página Negócio

**Volume diário de ordens de serviço** — requisições `POST api/v1/ordem-servico` concluídas sem erro.

```sql
SELECT count(*) AS 'OS criadas'
FROM Span
WHERE service.name = 'wrench-auto-repair-api'
  AND span.kind = 'server'
  AND http.route LIKE '%ordem-servico'
  AND http.request.method = 'POST'
  AND http.response.status_code < 400
SINCE 30 days ago
TIMESERIES 1 day
```

**Tempo médio de execução por status** — gauges publicados pela API no meter `Wrench.OrdemServico`
(`OrdemServicoMetrics`). Os valores são recalculados a cada 5 minutos por agregação no banco e
exibidos em minutos.

| Rótulo no painel | Métrica | Intervalo medido |
|---|---|---|
| Diagnóstico | `ordemservico.diagnostico.duration` | Criação da OS → diagnóstico concluído (inclui o tempo em `Recebida`) |
| Execução | `ordemservico.execucao.duration` | Aprovação do orçamento → finalização |
| Finalização | `ordemservico.entrega.duration` | Finalização → entrega ao cliente |

```sql
SELECT average(ordemservico.diagnostico.duration) / 60000 AS 'Diagnóstico',
       average(ordemservico.execucao.duration) / 60000 AS 'Execução',
       average(ordemservico.entrega.duration) / 60000 AS 'Finalização'
FROM Metric
WHERE service.name = 'wrench-auto-repair-api'
SINCE 7 days ago
TIMESERIES 1 hour
```

**Falhas no processamento de ordens de serviço** — contador `ordemservico.processamento.falhas`.
Incrementado pelo pipeline do MediatR `FalhaProcessamentoOrdemServicoBehavior` quando um comando de
`UseCases` do contexto de ordem de serviço lança exceção ou devolve erro `INESPERADO`. Falhas de
negócio (validação, não encontrado, conflito de status) não contam.

| Atributo | Conteúdo |
|---|---|
| `comando` | Nome do comando, por exemplo `FinalizarOrdemServicoCommand` |
| `tipo_erro` | Nome da exceção ou `INESPERADO` |

```sql
SELECT sum(ordemservico.processamento.falhas)
FROM Metric
WHERE service.name = 'wrench-auto-repair-api'
FACET comando, tipo_erro
SINCE 1 day ago
TIMESERIES 15 minutes
```

As métricas são exportadas com temporalidade delta, então `sum()` devolve as ocorrências do período.

### Página API

| Widget | Consulta |
|---|---|
| Latência p50, p95 e p99 | `SELECT percentile(duration.ms, 50, 95, 99) FROM Span WHERE service.name = 'wrench-auto-repair-api' AND span.kind = 'server' TIMESERIES` |
| Throughput | `SELECT rate(count(*), 1 minute) FROM Span WHERE ... AND span.kind = 'server' TIMESERIES` |
| Latência p95 por rota | `SELECT percentile(duration.ms, 95), count(*) FROM Span WHERE ... AND http.route IS NOT NULL FACET http.route` |
| Taxa de erro 5xx | `SELECT percentage(count(*), WHERE http.response.status_code >= 500) FROM Span WHERE ... AND span.kind = 'server' TIMESERIES 1 hour` |
| Erros por rota de ordem de serviço | Spans `server` com status >= 500 em rotas de `ordem-servico`, `diagnostico` e `orcamento`, agrupados por rota e status |

### Página Integrações

| Widget | O que mostra |
|---|---|
| Falhas em chamadas HTTP de saída | Spans `client` sem `db.system` com `otel.status_code = 'ERROR'` ou status >= 400, agrupados pelo host de destino. Cobre o envio de e-mail pelo SES, feito pelo AWS SDK sobre `HttpClient` |
| Falhas de banco de dados | Spans com `db.system` e `otel.status_code = 'ERROR'`, gerados pela instrumentação do EF Core |
| Logs de erro da aplicação | `SELECT count(*) FROM Log WHERE ... AND (level = 'Error' OR severity.text = 'Error') FACET message` |
| Duração média das chamadas de saída | Média de `duration.ms` dos spans `client`, total e só banco |

### Página Kubernetes

Dados do `nri-bundle` (repositório `infra-k8s`, pasta `newrelic-k8s`).

| Widget | Evento | Consulta base |
|---|---|---|
| CPU por pod da API | `K8sPodSample` | `average(cpuUsedCores) FACET podName` |
| Memória por pod da API | `K8sPodSample` | `average(memoryUsedBytes) / 1e6 FACET podName` |
| CPU e memória dos nós | `K8sNodeSample` | `average(cpuUsedCores), average(memoryUsedBytes) / 1e9 FACET nodeName` |
| Réplicas da API (HPA) | `K8sPodSample` | `uniqueCount(podName) WHERE status = 'Running' FACET namespaceName` |
| Restarts de container da API | `K8sContainerSample` | `sum(restartCountDelta) FACET podName` |
| Pods da API em Running | `K8sPodSample` | `percentage(count(*), WHERE status = 'Running')` |

Pods da API são filtrados por `podName LIKE 'wrench-api%'`.

### Página Healthcheck

`/health` e `/health/ready` ficam fora do tracing (`ObservabilityConfiguration`), porque as probes do
Kubernetes chamam esses endpoints a cada poucos segundos. A disponibilidade é medida de fora do
cluster pelo synthetic monitor `Wrench API - healthcheck`: tipo `SIMPLE`, a cada 5 minutos, a partir
de `US_EAST_1` e `SA_EAST_1`, contra `https://api.bgt3.com.br/health`, exigindo o texto `Healthy` na
resposta. Isso cobre falhas de DNS, ALB e certificado que os sinais internos não enxergam.

```sql
SELECT percentage(count(*), WHERE result = 'SUCCESS') AS 'Uptime (%)'
FROM SyntheticCheck
WHERE monitorName = 'Wrench API - healthcheck'
SINCE 1 day ago
```

A página também mostra o tempo de resposta por localização e a lista de execuções com falha.

---

## Alertas

Política `Wrench Auto Repair`, uma issue por condição (`PER_CONDITION`). O workflow
`Wrench Auto Repair - notificacao de incidentes` envia as issues da política por e-mail para o
endereço da variável `alert_email`.

| Condição | Sinal | Dispara quando |
|---|---|---|
| Falha no processamento de ordem de servico | `sum(ordemservico.processamento.falhas)` | Qualquer falha em uma janela de 5 minutos |
| Taxa de erro 5xx da API | Percentual de spans `server` com status >= 500 | Acima de `taxa_erro_5xx_limite_percentual` (5%) por 5 minutos |
| Latencia p95 da API | `percentile(duration.ms, 95)` dos spans `server` | Acima de `latencia_p95_limite_ms` (1500 ms) por 5 minutos |
| Restart de pod da API | `sum(restartCountDelta)` dos containers da API | Qualquer restart em uma janela de 5 minutos |
| Healthcheck externo da API falhando | Execuções `FAILED` do synthetic monitor | Falha em duas execuções seguidas (10 minutos) |

## Consultas úteis para investigação

Seguir uma requisição pelo identificador de correlação de negócio:

```sql
SELECT timestamp, message, level, trace.id
FROM Log
WHERE service.name = 'wrench-auto-repair-api' AND CorrelationId = '<valor do header X-Correlation-ID>'
SINCE 1 day ago
```

Spans de uma ordem de serviço com falha a partir do `trace.id` de um log:

```sql
SELECT name, duration.ms, otel.status_code, db.statement
FROM Span
WHERE trace.id = '<trace.id>'
SINCE 1 day ago
```
