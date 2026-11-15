# ADR 007 - Saga orquestrada no OS Service, com outbox transacional sobre SNS e SQS

## Status

Aceito

## Contexto

A Fase 4 quebra o monólito modular em três microsserviços com repositório, pipeline e banco
próprios:

| Serviço | Banco | Responsabilidade |
|---|---|---|
| **OS Service** (este repositório) | PostgreSQL `wrench_os` | Ciclo de vida da ordem de serviço, autenticação, cadastro e estoque |
| **Billing Service** | PostgreSQL `wrench_billing` | Orçamento, aprovação, cobrança Pix e estorno no Mercado Pago |
| **Execução Service** | DynamoDB | Fila de execução, diagnóstico e reparo |

Abrir uma ordem de serviço deixou de ser uma transação. O caminho completo — diagnóstico →
orçamento → aprovação do cliente → pagamento → execução → finalização — atravessa três bancos
diferentes, e nenhum serviço pode ler o banco do outro. Não existe `COMMIT` que cubra o conjunto:
qualquer etapa pode falhar, demorar indefinidamente ou simplesmente não responder, e o estado
precisa voltar para algo coerente — inclusive devolvendo dinheiro já cobrado.

Três decisões precisavam ser tomadas juntas, porque uma condiciona a outra: **quem coordena**,
**como as mensagens trafegam** e **como o estado local e a mensagem publicada não divergem**.

---

## Decisão

### 1. Saga orquestrada, com o orquestrador dentro do OS Service

A coordenação é **orquestrada**, não coreografada, e o orquestrador mora no OS Service, no bounded
context da ordem de serviço (`domain/Saga`, `application/Saga`, `infra/Saga`).

A máquina de estados é a entidade `SagaOrdemServico`, com estado, etapa corrente, prazo da etapa e
um histórico de transições. Cada mensagem recebida vira uma transição; transições inválidas ou
repetidas devolvem `false` e não mudam nada.

```
AguardandoOrcamento ──OrcamentoCriado──▶ AguardandoAprovacao ──OrcamentoAprovado──▶ AguardandoPagamento
                                                                                            │
                                                                            PagamentoAprovado│
                                                                                            ▼
                     Concluida ◀──ExecucaoConcluida── AguardandoExecucao ◀───────────────────
```

Qualquer falha ou prazo estourado leva a `Compensando` (quando há dinheiro a devolver) ou direto a
`Compensada`. Um estorno que não volta deixa a Saga em `Falha`, que exige tratamento manual — é um
estado explícito, não um silêncio.

### 2. Amazon SNS + SQS com DLQ, um tópico por publicador e uma fila por consumidor

O transporte é **SNS para publicar** e **SQS para consumir**, provisionado em Terraform no
`infra-k8s` (`terraform/mensageria`) e acessado por IRSA, sem chave estática.

* Um **tópico por publicador**: `wrench-os-events`, `wrench-billing-events`, `wrench-execucao-events`.
  Ninguém publica no tópico de outro.
* Uma **fila com DLQ por consumidor**: `wrench-billing-gerar-orcamento`,
  `wrench-billing-cancelar-orcamento`, `wrench-billing-estornar-pagamento`,
  `wrench-execucao-commands` e `wrench-os-saga`. Ninguém lê a fila de outro. O Billing tem uma fila
  por comando porque consome com MassTransit: sem o envelope do próprio MassTransit, descartado pelo
  `raw_message_delivery`, o tipo da mensagem só pode ser inferido pela fila em que ela chegou.
* A assinatura usa `raw_message_delivery` e um **filter policy sobre o atributo `tipo`**, de modo
  que cada consumidor só recebe o que sabe tratar.
* Esgotadas as entregas (`maxReceiveCount = 5`), a mensagem vai para a DLQ e o alerta
  **DLQ da Saga com mensagens** abre incidente no New Relic.

Os contratos vivem no pacote versionado [`Wrench.Contracts`](https://github.com/euthribeiro/wrench-contracts),
com mudanças apenas aditivas na linha 1.x.

### 3. Outbox e inbox transacionais no mesmo `DbContext`

Mudar o estado no Postgres e publicar no SNS são duas operações que podem falhar
independentemente. Para que não divirjam:

* o orquestrador **enfileira a mensagem de saída no mesmo `OrdemServicoDbContext`** que grava o
  novo estado da Saga e da OS; um único `CommitAsync` cobre os três (tabela `SagaOutbox`);
* o `OutboxDispatcher` publica as pendentes no SNS e as marca como publicadas. Uma falha de
  publicação mantém a mensagem pendente, o que dá **entrega ao menos uma vez**;
* do outro lado, o consumidor grava o `messageId` em `SagaInbox` **na mesma transação** da mudança
  de estado, o que descarta entregas repetidas — **idempotência**;
* o `SagaTimeoutWorker` varre as Sagas com prazo vencido e aplica a **mesma compensação** que a
  falha equivalente aplicaria.

### 4. Compensação por etapa

| Etapa que falha | Evento ou timeout | Compensação emitida | Status final da OS |
|---|---|---|---|
| Geração do orçamento | `OrcamentoFalhou` | nenhuma: nada foi criado fora | `EmDiagnostico` |
| Aprovação do cliente | `OrcamentoRecusado` | `CancelarOrcamento` | `Cancelada` |
| Pagamento | `PagamentoFalhou` | `CancelarOrcamento` | `Cancelada` |
| Execução | `ExecucaoFalhou` | `CancelarExecucao` + `EstornarPagamento` | `Cancelada` |
| Estorno | `EstornoFalhou` ou timeout | nenhuma: Saga em `Falha` | `Cancelada` |

A falha ao gerar o orçamento é a única que **não** cancela a ordem de serviço: nada foi prometido
ao cliente e nada foi cobrado, então a OS volta para `EmDiagnostico` e o atendente tenta de novo.

---

## Alternativas descartadas

### Saga coreografada

Cada serviço reagiria ao evento do anterior, sem coordenador. Descartada porque a regra de
compensação ficaria espalhada: para saber por que uma OS foi cancelada seria preciso ler o código
dos três serviços e correlacionar logs. Com o orquestrador, o estado da Saga está em uma tabela, o
histórico de transições é consultável e as compensações são testáveis em memória — como mostram os
testes de `SagaOrdemServico` e `SagaOrquestrador`. O custo aceito é que o OS Service vira ponto
central: se ele cai, nada avança. O outbox e a DLQ limitam o estrago, porque nada se perde
enquanto ele está fora.

### Orquestrador como quarto serviço

Separaria a coordenação do ciclo de vida da OS. Descartada porque o estado da Saga e o status da
ordem de serviço mudam juntos: separá-los exigiria uma transação distribuída entre dois bancos
para resolver exatamente o problema que a Saga existe para evitar. Dentro do OS Service, os dois
cabem no mesmo `CommitAsync`.

### MassTransit com state machine

Era a opção do plano original e traz saga, outbox e retry prontos. Descartada na implementação por
dois motivos. Primeiro, o `execucao-service` já estava publicando e consumindo com o SDK da AWS e
atributos de mensagem próprios (`tipo`, `correlationId`, `traceparent`); o transporte SQS do
MassTransit cria a sua própria topologia e nomes de tópico derivados do tipo .NET, o que não
conversa com isso sem configuração extra. Segundo, a topologia que queríamos — um tópico por
publicador e filtro por atributo — é explícita no Terraform e fica visível para quem opera. O
custo aceito é ter escrito o outbox, o dispatcher e o consumidor à mão: cerca de 300 linhas, todas
cobertas por teste.

### RabbitMQ ou Kafka no cluster

Descartados por custo operacional. Seriam mais um workload com estado para manter no EKS dentro de
um prazo de duas semanas, enquanto SNS e SQS são gerenciados, cabem no free tier, já têm DLQ e
redrive nativos e se integram por IRSA.

### Publicar direto no SNS dentro do handler, sem outbox

Seria mais simples. Descartado porque a falha é silenciosa e assimétrica: se o `COMMIT` passa e o
`Publish` falha, a OS fica em um estado que o resto do sistema nunca ouve falar; se o `Publish`
passa e o `COMMIT` falha, o Billing cobra por uma OS que não existe. O outbox custa uma tabela e
um worker, e elimina as duas janelas.

---

## Consequências

### Positivas

* O estado da Saga é uma linha em `SagaOrdemServico`, com histórico de transições — dá para
  responder "por que esta OS foi cancelada" com uma consulta.
* Nenhuma mensagem se perde: ou está no outbox, ou na fila, ou na DLQ com alerta.
* Mensagem repetida é inofensiva, por `SagaInbox` e pelas guardas da máquina de estados.
* Um serviço que nunca responde não trava a OS: o timeout aplica a compensação.
* O `traceparent` viaja no message attribute, então o trace distribuído no New Relic liga a
  abertura da OS ao estorno no Mercado Pago.

### Negativas

* O OS Service concentra o caminho crítico. É o risco anotado no plano de ação e foi aceito.
* Entrega ao menos uma vez empurra a idempotência para o consumidor; cada serviço precisa manter
  o seu inbox.
* A ordem das mensagens não é garantida pelo SQS padrão. A máquina de estados tolera isso
  recusando transições fora de ordem, mas não é a mesma coisa que ordenação forte — se ela passar
  a ser necessária, a saída é fila FIFO por `ordemServicoId`.
* O outbox adiciona latência da ordem do intervalo do dispatcher (2 segundos por padrão) entre a
  mudança de estado e a publicação.
* A compensação é semântica, não um rollback: uma OS cancelada depois do pagamento fica
  `Cancelada` com estorno registrado, e não volta a ser como se nunca tivesse existido.

---

## Referências

* [README da Saga](../saga.md) — fluxo, eventos, configuração e como testar
* [`infra-k8s/terraform/mensageria`](https://github.com/euthribeiro/infra-k8s/tree/master/terraform/mensageria) — tópicos, filas, DLQ e IRSA
* [`Wrench.Contracts`](https://github.com/euthribeiro/wrench-contracts) — contratos versionados
* [ADR 005](./ADR%20005%20-%20Padrao%20de%20Comunicacao.md) — padrão de comunicação da Fase 3, que esta decisão estende
