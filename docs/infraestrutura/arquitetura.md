# Arquitetura — Wrench Auto Repair

Visão de nuvem da solução: borda autenticada com API Gateway e funções serverless, aplicação em
Kubernetes (EKS), banco gerenciado (RDS PostgreSQL) e observabilidade no New Relic. Todo recurso é
provisionado com Terraform, com estado no HCP Terraform, e entregue por quatro repositórios com
CI/CD próprio no GitHub Actions.

## Diagrama de componentes

```mermaid
flowchart TB
    user(["Cliente / Atendente"])

    subgraph CF["Cloudflare (DNS)"]
        dnsApp["api.bgt3.com.br<br/>hml-api.bgt3.com.br"]
        dnsDb["prod-db.bgt3.com.br"]
    end

    subgraph AWS["AWS — us-east-1"]
        subgraph EDGE["Borda autenticada"]
            apigw["API Gateway HTTP API<br/>wrench-api-gateway-{env}"]
            authz["Lambda authorizer<br/>wrench-auth-authorizer-{env}<br/>valida JWT"]
            lambda["Lambda de autenticação<br/>wrench-auth-cpf-{env}<br/>CPF, status, emite JWT"]
            cw["CloudWatch Logs<br/>logs JSON das Lambdas"]
        end

        acm["ACM<br/>certificado HTTPS"]
        ecr["ECR<br/>imagem + chart OCI"]
        ses["SES<br/>e-mail transacional"]

        subgraph VPC["VPC 10.0.0.0/16"]
            alb["ALB internet-facing<br/>AWS Load Balancer Controller"]

            subgraph EKS["EKS — eks-wrench-auto-repair"]
                subgraph GWNS["namespace gateway"]
                    gw["Gateway bgt3-gw<br/>plataforma, rotas de todos os namespaces"]
                end
                subgraph PROD["namespace production"]
                    routeProd["HTTPRoute<br/>api.bgt3.com.br"]
                    api["Deployment wrench-api<br/>.NET 10, HPA 1 a 6"]
                end
                subgraph HML["namespace homologacao"]
                    routeHml["HTTPRoute<br/>hml-api.bgt3.com.br"]
                    apiHml["Deployment wrench-api<br/>release wrench-hml"]
                end
                subgraph NRNS["namespace newrelic"]
                    nri["nri-bundle<br/>infra agent, kube-state-metrics<br/>eventos, Fluent Bit"]
                end
                ms["Metrics Server"]
            end
        end

        rds[("RDS PostgreSQL 18<br/>wrench_auto_repair<br/>wrench_auto_repair_hml")]
    end

    subgraph NR["New Relic"]
        ingest["Ingest<br/>OTLP e Infrastructure"]
        dash["Dashboards"]
        alerts["Alert policies"]
        synth["Synthetic monitor<br/>GET /health"]
    end

    user -->|"POST /auth/cpf"| apigw
    apigw -->|"AWS_PROXY"| lambda
    lambda -->|"EF Core somente leitura<br/>GRANT por coluna"| dnsDb
    user -->|"Bearer JWT em /api"| apigw
    apigw -.->|"autoriza"| authz
    apigw -->|"HTTP_PROXY"| dnsApp
    dnsApp --> alb
    acm -.->|"certificado api e hml-api"| alb
    alb --> gw
    gw --> routeProd --> api
    gw --> routeHml --> apiHml
    api -->|"revalida JWT e roles<br/>EF Core, SSL"| dnsDb
    apiHml --> dnsDb
    dnsDb --> rds
    api -.->|"IRSA"| ses
    ecr -.->|"imagem"| api
    ms -.->|"CPU para o HPA"| api
    api -.->|"OTLP: traces, métricas, logs"| ingest
    nri -.->|"CPU, memória, eventos, logs"| ingest
    lambda -.->|"logs"| cw
    authz -.->|"logs"| cw
    ingest --> dash
    ingest --> alerts
    synth -.->|"uptime"| dnsApp
```

Linhas sólidas representam o caminho da requisição e dos dados; linhas pontilhadas representam
relações de controle, telemetria e provisionamento. `{env}` é `homologacao` ou `production`.

## Borda autenticada

O API Gateway é o ponto de entrada documentado para consumidores. Cada ambiente tem o seu gateway,
e o destino do `HTTP_PROXY` é o hostname da API naquele ambiente.

| Rota | Integração | Authorizer | Destino |
|---|---|---|---|
| `POST /auth/cpf` | `AWS_PROXY` | não | Lambda de autenticação |
| `ANY /api/{proxy+}` | `HTTP_PROXY` | Lambda authorizer | `https://api.bgt3.com.br/api/{proxy}` |
| Login por e-mail e senha da API | `HTTP_PROXY` | não | endpoint de autenticação da API |
| `GET /health` | `HTTP_PROXY` | não | liveness da API |
| Documentação Scalar e OpenAPI | `HTTP_PROXY` | não | `/docs-ui` e `/openapi` da API |

**Lambda de autenticação.** Recebe o CPF, valida os dígitos verificadores, consulta o cliente, o
usuário vinculado e o perfil, recusa usuário inativo e emite um JWT HS256.

**Lambda authorizer.** Recebe o cabeçalho `Authorization`, valida assinatura, emissor, audiência e
expiração, e devolve a decisão ao gateway. O JWT authorizer nativo do API Gateway HTTP exige chaves
publicadas por JWKS e não aceita HS256, por isso a validação é feita por função
([ADR 006](../adrs/ADR%20006%20-%20Lambda%20Authorizer.md)).

**Defesa em profundidade.** O ALB continua alcançável pela internet. A API revalida o token e as
roles de cada endpoint, então uma requisição que não passe pelo gateway recebe a mesma recusa.

## Contratos entre componentes

| Contrato | Entre | Definição | Garantia |
|---|---|---|---|
| Token JWT | Lambda de autenticação, authorizer e API | `Issuer` e `Audience` `Wrench Auto Repair`, chave `JWT_SIGNING_KEY`, claims `NameIdentifier`, `Name` e `Role` | mesmos valores injetados por secret nos dois repositórios |
| Colunas lidas pela Lambda | Lambda de autenticação e schema da API | `Clientes(Id, Documento, Email)`, `Usuarios(Id, Email, PerfilId, Ativo)`, `Perfis(Id, Nome)` | teste de contrato no `app-k8s` e `GRANT SELECT` por coluna no `infra-db` |
| Outputs de infraestrutura | repositórios de infraestrutura e consumidores | `vpc_id`, `public_subnet_ids`, `cluster_name`, `database_hostname`, `database_name`, repositórios ECR | remote state do HCP Terraform |

O modelo de dados completo e os privilégios de cada role estão em
[Modelo relacional](../database/modelo-relacional.md).

## Observabilidade

| Sinal | Origem | Destino | Uso |
|---|---|---|---|
| Traces HTTP, EF Core e HttpClient | OpenTelemetry na API | New Relic via OTLP | latência por rota, erros de integração |
| Métricas de runtime e de negócio | OpenTelemetry na API (`ordemservico.*.duration`, `ordemservico.processamento.falhas`) | New Relic via OTLP | tempo médio por fase, alertas de falha de OS |
| Logs JSON com `CorrelationId`, `TraceId` e `SpanId` | Serilog na API | stdout e New Relic via OTLP | investigação correlacionada |
| CPU, memória, eventos e logs do cluster | `nri-bundle` | New Relic Infrastructure | consumo de recursos e restarts |
| Uptime | Synthetic monitor em `/health` | New Relic | disponibilidade vista de fora do cluster |
| Logs das Lambdas | runtime Lambda em formato JSON | CloudWatch Logs | autenticação e autorização |

Dashboards, alert policies e o synthetic monitor são declarados em Terraform no repositório
`infra-k8s` (`terraform/observability`). As queries estão descritas em
[dashboards NRQL](../observability/dashboards-nrql.md) e a decisão de ferramenta no
[ADR 003](../adrs/ADR%20003%20-%20Observabilidade%20com%20New%20Relic.md).

## Repositórios e fronteiras

```mermaid
flowchart LR
    subgraph r2["infra-k8s"]
        c2["VPC, EKS, ACM, ECR<br/>SES, DNS, Structurizr<br/>nri-bundle, New Relic como código"]
    end
    subgraph r3["infra-db"]
        c3["RDS PostgreSQL<br/>roles da API e da Lambda"]
    end
    subgraph r4["app-k8s"]
        c4["API .NET 10<br/>chart Helm"]
    end
    subgraph r1["lambda-auth"]
        c1["Lambda de autenticação<br/>Lambda authorizer<br/>API Gateway"]
    end

    c2 -->|"vpc_id, public_subnet_ids"| c3
    c2 -->|"cluster, ACM, ECR, role IRSA do SES"| c4
    c3 -->|"database_hostname, database_name"| c4
    c3 -->|"database_hostname, database_name"| c1
    c4 -.->|"contrato de JWT e de colunas"| c1
    c1 -->|"HTTP_PROXY para o hostname da API"| c4
```

Os valores atravessam repositórios por **remote state do HCP Terraform**. Nenhum pipeline dispara
outro; cada consumidor lê o estado atual da infraestrutura no momento do próprio deploy.

## Fluxo de deploy (CI/CD)

```mermaid
flowchart TB
    subgraph PR["Pull request em qualquer repositório"]
        val["build, testes, auditoria de vulnerabilidades<br/>terraform fmt, validate, plan, helm lint"]
    end

    subgraph IK["infra-k8s — push em master"]
        i1["infra, ecr, email"] --> i2["structurizr, dns"]
        i1 --> i3["nri-bundle, observability"]
    end

    subgraph ID["infra-db — push em master"]
        d1["rds"] --> d2["roles"]
    end

    subgraph AK["app-k8s — develop e master"]
        a1["build, testes, auditoria"] --> a2["lê outputs por remote state"]
        a2 --> a3["docker build e push no ECR"]
        a3 --> a4["helm package e push OCI"]
        a4 --> a5["helm upgrade no namespace do ambiente"]
    end

    subgraph LA["lambda-auth — develop e master"]
        l1["build, testes, auditoria"] --> l2["dotnet publish e pacote zip"]
        l2 --> l3["terraform apply<br/>workspace do ambiente"]
    end

    PR -.->|"gate obrigatório"| IK
    PR -.->|"gate obrigatório"| ID
    PR -.->|"gate obrigatório"| AK
    PR -.->|"gate obrigatório"| LA
```

| Repositório | `develop` | `master` |
|---|---|---|
| `app-k8s` | namespace `homologacao`, `hml-api.bgt3.com.br`, database `wrench_auto_repair_hml` | namespace `production`, `api.bgt3.com.br`, database `wrench_auto_repair` |
| `lambda-auth` | Lambdas e gateway de homologação | Lambdas e gateway de produção |
| `infra-k8s`, `infra-db` | PR roda `plan` | `apply` da infraestrutura compartilhada |

Homologação e produção compartilham cluster, Gateway de plataforma, ALB e instância RDS. A
segregação é por namespace, release Helm, hostname, database e API Gateway
([ADR 004](../adrs/ADR%20004%20-%20Uso%20de%20HPA.md)).

### Ordem do primeiro provisionamento

O **Orquestrador de Provisionamento** do `infra-k8s` executa a sequência a partir de um único
*Run workflow*, disparando o pipeline de cada repositório e aguardando o anterior terminar:

1. `infra-k8s`: rede, cluster, ECR, e-mail, Gateway de plataforma, DNS e observabilidade.
2. `infra-db`: instância RDS, databases de produção e de homologação e roles.
3. `app-k8s`: deploy de homologação e depois de produção; as migrations criam as tabelas em cada
   database.
4. `infra-db`: roles novamente, concedendo à Lambda o `SELECT` por coluna nos dois databases.
5. `lambda-auth`: Lambdas e API Gateway dos dois ambientes.

A concessão por coluna só é aceita com a tabela existente, o que impõe o passo 4 depois do 3.

### Destruição

O **Orquestrador de Destruição** do `infra-k8s` percorre a ordem inversa: `lambda-auth`, `app-k8s`
(o `destroy.yml` de cada ambiente remove release e namespace), `infra-db` e `infra-k8s`. Remover a
aplicação antes do cluster evita recursos órfãos ligados ao Gateway e ao ALB.

## Componentes por camada

| Camada | Recursos | Provisionado por | Repositório |
|---|---|---|---|
| **Borda autenticada** | API Gateway HTTP API, Lambda de autenticação, Lambda authorizer, log groups | Terraform | `lambda-auth` |
| **DNS** | CNAMEs da API e do banco, validação ACM, DKIM | Terraform (Cloudflare) | `infra-k8s` / `infra-db` |
| **Rede** | VPC, subnets públicas e privadas, IGW, NAT, rotas, security groups | Terraform (`infra/`) | `infra-k8s` |
| **Cluster** | EKS, node group, OIDC, addons, controllers, CRDs do Gateway API | Terraform (`infra/`) | `infra-k8s` |
| **Segurança/IAM** | Roles do cluster e dos nós, IRSA (EBS, LBC, SES), roles de execução das Lambdas | Terraform | `infra-k8s` / `lambda-auth` |
| **Dados** | RDS PostgreSQL 18 | Terraform (`rds/`) | `infra-db` |
| **Dados** | Role da API e role somente leitura da Lambda | Terraform (`roles/`) | `infra-db` |
| **Registry** | Repositórios ECR de imagem e de chart | Terraform (`ecr/`) | `infra-k8s` |
| **Entrada HTTP** | Gateway `bgt3-gw` no namespace `gateway`, LoadBalancerConfiguration, ALB | pipeline de infraestrutura | `infra-k8s` |
| **Aplicação** | Deployment, Service, HPA, HTTPRoutes, TargetGroupConfiguration, Secrets, ServiceAccount | Helm (`wrench-api-k8s/`) | `app-k8s` |
| **Observabilidade (cluster)** | `nri-bundle` | Helm pelo pipeline | `infra-k8s` |
| **Observabilidade (plataforma)** | Dashboards, alert policies, synthetic monitor | Terraform (`observability/`) | `infra-k8s` |
| **Observabilidade (aplicação)** | Serilog, OpenTelemetry, CorrelationId, métricas de negócio | código | `app-k8s` |
| **Entrega** | Build, testes, auditoria, deploy | GitHub Actions | todos |

> Diagramas em [Mermaid](https://mermaid.js.org), renderizados pelo GitHub. A versão navegável está
> em [`arquitetura.html`](./arquitetura.html) e o modelo C4 em
> [`structurizr/workspace.dsl`](./structurizr/workspace.dsl).
