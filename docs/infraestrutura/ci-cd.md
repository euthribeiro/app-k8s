# CI/CD — Aplicação (`app-k8s`)

Pipelines deste repositório. Não existe workflow orquestrador: cada um dos quatro repositórios tem
o seu próprio ciclo, e os valores que atravessam a fronteira vêm de remote state do HCP Terraform.

## Workflows

| Arquivo | Gatilho | O que faz |
|---|---|---|
| `pull-request.yml` | PR para `master` ou `develop` | Build, auditoria de vulnerabilidades, testes, `helm lint` e `helm template` |
| `ci-cd.yml` | push em `develop` ou `master` | Chama o build e, conforme a branch, o deploy do ambiente correspondente |
| `dotnet-ci.yml` | `workflow_call` | Restore, auditoria, build e testes |
| `deploy.yml` | `workflow_call` | Lê os outputs de infra, versiona, publica imagem e chart, aplica com Helm |

`dotnet-ci.yml` inclui o teste de contrato do banco com a Lambda de autenticação: depois das
migrations, ele verifica que as colunas lidas pela Lambda existem com o tipo esperado. Uma migration
que renomeie ou remova uma delas falha o pipeline da aplicação.

## Ambientes

| Branch | Ambiente | Namespace | Release Helm | Hostname |
|---|---|---|---|---|
| `develop` | `homologacao` | `homologacao` | `wrench-hml` | `hml-api.bgt3.com.br` |
| `master` | `production` | `production` | `wrench` | `api.bgt3.com.br` |

Os dois compartilham cluster e banco. Não há EKS nem RDS duplicados — a decisão de custo e as
suas consequências estão no [ADR 004](../adrs/ADR%20004%20-%20Uso%20de%20HPA.md).

O job de deploy declara `environment: ${{ inputs.environment }}`, então secrets e variáveis podem
ser diferenciados por ambiente nas configurações do repositório, e o GitHub registra o histórico
de deploy por ambiente.

## Auditoria de dependências

`dotnet-ci.yml` roda `dotnet list package --vulnerable --include-transitive` e **falha o build** se
o relatório listar qualquer pacote. É o mesmo comando que se roda localmente:

```bash
dotnet list wrench.auto.repair/wrench.auto.repair.sln package --vulnerable --include-transitive
```

## Como os valores de infraestrutura chegam ao deploy

O passo _Ler outputs de infra-k8s e infra-db_ aplica o módulo auxiliar
[`.github/terraform-outputs`](../../.github/terraform-outputs/main.tf), que só declara
`terraform_remote_state` e reexpõe o que o `helm upgrade` precisa. Ele roda com backend local
(state efêmero do runner) e não cria nem altera recurso nenhum.

| Valor | Workspace de origem | Repositório |
|---|---|---|
| `cluster_name`, `acm_certificate_arn` | `wrench_auto_repair` | `infra-k8s` |
| `api_repository`, `chart_repository`, `chart_repository_url` | `wrench_auto_repair_ecr` | `infra-k8s` |
| `app_ses_irsa_role_arn` | `wrench_auto_repair_email` | `infra-k8s` |
| `database_hostname`, `database_name` | `wrench_auto_repair_rds` | `infra-db` |

Isso exige que o **state sharing** esteja habilitado no HCP para os quatro workspaces. Sem isso o
`terraform apply` do módulo auxiliar falha na leitura, e o deploy para antes de tocar no cluster.

## Sequência do deploy

1. Checkout com `fetch-depth: 0` (a action de tag precisa do histórico)
2. Cálculo da próxima versão semântica a partir dos commits
3. Leitura dos outputs de infraestrutura
4. Login no ECR
5. `aws eks update-kubeconfig`
6. `docker build` e `docker push` da imagem
7. `helm package` e `helm push` do chart (OCI)
8. `helm upgrade --install` com os valores injetados, incluindo `jwt.signingKey`,
   `application.admin.password` e `newRelic.licenseKey`
9. `kubectl wait --for=condition=Programmed gateway/bgt3-gw`

O passo 9 é o que torna o `terraform apply` do stack `dns/` (no `infra-k8s`) idempotente: o CNAME
só pode apontar para um ALB que já existe.

## Variáveis e secrets

| Nome | Tipo | Descrição |
|---|---|---|
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` | secret | Credenciais de deploy |
| `AWS_REGION` | variable | Região AWS |
| `TF_API_TOKEN` | secret | Token do HCP Terraform (leitura dos states) |
| `TFC_ORGANIZATION` | variable | Organização no HCP (`bgt3`) |
| `TFC_WORKSPACE_INFRA` | variable | `wrench_auto_repair` |
| `TFC_WORKSPACE_RDS` | variable | `wrench_auto_repair_rds` |
| `TFC_WORKSPACE_EMAIL` | variable | `wrench_auto_repair_email` |
| `TFC_WORKSPACE_ECR` | variable | `wrench_auto_repair_ecr` |
| `APPLICATION_DATABASE_USERNAME` / `APPLICATION_DB_PASSWORD` | secret | Role de menor privilégio no RDS |
| `ADMIN_EMAIL` | variable | E-mail do administrador semeado |
| `ADMIN_PASSWORD` | secret | Senha do administrador |
| `JWT_ISSUER` / `JWT_AUDIENCE` | variable | Emissor e audiência do token |
| `JWT_SIGNING_KEY` | secret | Chave de assinatura, compartilhada com o `lambda-auth` |
| `NEW_RELIC_LICENSE_KEY` | secret | Ingest license key do New Relic, injetada no secret `newrelic-credentials` |
| `NO_REPLY_EMAIL` | secret | Remetente das notificações |
| `AWS_ACCESS_KEY_ID_SES` / `AWS_SECRET_ACCESS_KEY_SES` | secret | Credenciais usadas nos testes |

Nenhum desses valores tem default no repositório: o chart falha explicitamente quando
`application.admin.password` ou `jwt.signingKey` chegam vazios, e a aplicação recusa subir sem
chave de assinatura configurada. Sem `NEW_RELIC_LICENSE_KEY` a aplicação sobe normalmente e o New
Relic recusa a exportação.

## Pipelines dos outros repositórios

| Repositório | `develop` | `master` | O que aplica |
|---|---|---|---|
| `infra-k8s` | — | `apply` | `infra`, `ecr`, `email`, `structurizr`, `dns`, `nri-bundle` e `observability` (dashboards, alertas e synthetic monitor) |
| `infra-db` | — | `apply` | `rds` e, em seguida, `roles` (role da API e role somente leitura da Lambda) |
| `lambda-auth` | homologação | produção | build e testes, pacote da função e Terraform das duas Lambdas e do API Gateway, em workspace por ambiente |

Em todos, PR roda validação (`fmt`, `validate`, `plan`, build e testes) e a branch principal é
protegida, exigindo Pull Request.

## Primeiro provisionamento

1. `infra-k8s`: `infra`, `ecr`, `email`.
2. `infra-db`: `rds` e `roles`.
3. `app-k8s`: primeiro deploy, que cria as tabelas pelas migrations.
4. `infra-db`: `roles` novamente, para o `GRANT SELECT` por coluna da Lambda.
5. `infra-k8s`: `dns`, `nri-bundle` e `observability`.
6. `lambda-auth`: Lambdas e API Gateway.
