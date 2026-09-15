# Pipeline HTTP da API

Ordem dos middlewares registrados em `src/wrench.web.api/Program.cs` e as restrições que a
sustentam. A ordem não é arbitrária: trocar dois registros de lugar quebra comportamento
observável em produção.

## Ordem e restrições

| # | Registro | Restrição |
|---|---|---|
| 1 | `UseDeveloperExceptionPage` / `UseGlobalExceptionHandler` | Primeiro da cadeia — precisa envolver todo o resto para capturar exceções lançadas abaixo. Em `Development` usa a página detalhada; nos demais ambientes, o handler que devolve `ProblemDetails` sem vazar stack trace. |
| 2 | `UseForwardedHeaders` | **Precisa vir antes de `MapScalarApiReference`.** A aplicação roda atrás do ALB, que termina o TLS; sem reescrever `X-Forwarded-Proto` e `X-Forwarded-For` antes, o Scalar monta as URLs da documentação em `http://` e o navegador bloqueia o conteúdo misto. |
| 3 | `MapOpenApi` + `MapScalarApiReference("docs-ui")` | Publica o documento OpenAPI e a UI do Scalar. Depende do passo anterior para gerar links absolutos corretos. |
| 4 | `UseAuthorization` | Depois do roteamento e antes de `MapControllers`. |
| 5 | `MapControllers` | Endpoints da aplicação. Nomes de rota passam por `SlugifyParameterTransformer`. |
| 6 | `MapGet("/")` | Redirect permanente para `/docs-ui`. |
| 7 | `MapHealthChecks` | `/health` (tag `live`) e `/health/ready` (tag `ready`). |

## Forwarded headers e o proxy

`ForwardedHeadersOptions` limpa `KnownIPNetworks` e `KnownProxies`. Isso desliga a validação de
origem dos cabeçalhos, o que só é aceitável porque o único caminho de entrada do tráfego é o ALB
provisionado pelo AWS Load Balancer Controller — o pod não é exposto diretamente. Se a topologia
mudar e o pod passar a receber tráfego de outra origem, essa configuração precisa ser revista,
porque um cliente arbitrário poderia forjar `X-Forwarded-For`.

## Healthchecks

Dois endpoints, com propósitos distintos:

- **`/health`** (tag `live`) — *liveness*. Responde `Healthy` fixo, sem tocar no banco. Serve para
  o Kubernetes saber se o processo está vivo; se dependesse do banco, uma indisponibilidade do RDS
  causaria reinício em massa dos pods sem resolver nada.
- **`/health/ready`** (tag `ready`) — *readiness*. Verifica os quatro `DbContext` (conectividade
  via `AddDbContextCheck` e migrations pendentes via `DatabaseMigrationHealthCheck`). Só um pod com
  schema atualizado entra no balanceador. `AllowCachingResponses = false` e `Degraded` mapeado para
  `200` para não tirar o pod de rotação por degradação parcial.

## Migrations no start

`ApplyMigrationsAsync()` e `UseSeeds()` rodam entre `builder.Build()` e o registro dos
middlewares — ou seja, antes de a aplicação aceitar qualquer requisição. Detalhes dos contextos e
comandos em [`docs/database/migrations.md`](../database/migrations.md).
