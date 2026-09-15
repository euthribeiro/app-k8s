# Kubernetes — Manifestos e deploy da aplicação

Este documento descreve os recursos Kubernetes da aplicação _Wrench Auto Repair_
e como aplicá-los. Há **dois caminhos**:

- **`kubernetes/reference/`** — manifestos YAML "crus", usados como **referência** de leitura e
  para aplicação manual/local (`kubectl apply`). Atendem ao requisito de _"criar manifestos YAML
  para deploy em Kubernetes"_ da Fase 2.
- **`wrench-api-k8s/`** — **chart Helm** (caminho de deploy real, usado pelo CI/CD). Os mesmos
  recursos, parametrizados via `values.yaml` e injetados pela pipeline.

> O banco de dados é o **RDS gerenciado**, provisionado pelo repositório `infra-db`, com um database
> por ambiente na mesma instância: `wrench_auto_repair` (produção) e `wrench_auto_repair_hml`
> (homologação). Não existe Postgres dentro do cluster.

---

## Entrada HTTP compartilhada

O `Gateway` `bgt3-gw`, a `LoadBalancerConfiguration` e o ALB **não pertencem a este repositório**.
São recursos de plataforma do `infra-k8s`, no namespace `gateway`, com listeners `http` (80) e
`https` (443) que aceitam rotas de qualquer namespace e um certificado ACM que cobre
`api.bgt3.com.br` e `hml-api.bgt3.com.br`.

A aplicação publica apenas as suas `HTTPRoute`, com `parentRefs` apontando para
`gateway/bgt3-gw` e filtradas pelo hostname do ambiente. Por isso homologação e produção
compartilham um único ALB sem conflito de recursos.

## Recursos criados por ambiente

Os nomes são iguais nos dois ambientes; o que os separa é o namespace (`production` ou
`homologacao`), criado pelo `helm upgrade --create-namespace` e removido pelo workflow de
destruição.

| Recurso | Kind | Papel |
|---------|------|-------|
| `wrench-api-sa` | `ServiceAccount` | SA da API; anotada com a role IRSA do SES (envio de e-mail sem chave estática) |
| `db-credentials` | `Secret` | Connection string do database do ambiente e configuração da aplicação |
| `newrelic-credentials` | `Secret` | Endpoint, protocolo e cabeçalho de autenticação do OTLP do New Relic |
| `wrench-api-deployment` | `Deployment` | API .NET, porta 8080, RollingUpdate, probes `/health` e `/health/ready` |
| `wrench-api-svc` | `Service` (ClusterIP) | Expõe a API na porta 8080 dentro do cluster |
| `wrench-api-hpa` | `HorizontalPodAutoscaler` | Escala a API por CPU (50%), 1–6 réplicas |
| `wrench-api-tg` | `TargetGroupConfiguration` (CRD AWS) | `targetType: ip` para o target group do ALB |
| `wrench-http-route` | `HTTPRoute` | Roteia o hostname do ambiente para o `wrench-api-svc` (listener `https`) |
| `wrench-https-redirect` | `HTTPRoute` | Redirect 301 HTTP → HTTPS para o hostname do ambiente (listener `http`) |

O manifesto `wrench-production-namespace.yaml` existe apenas para a aplicação manual dos manifestos
crus; no chart, o namespace é criado pelo Helm.

### Detalhes de escalabilidade (HPA)

O HPA escala por utilização de CPU (alvo 50%), entre `minReplicas: 1` e
`maxReplicas: 6`. O comportamento é assimétrico: sobe rápido (até +100% a cada
30s, sem janela de estabilização) e desce devagar (−50% a cada 60s, com janela de
300s), evitando _flapping_. Depende do **Metrics Server** (instalado como addon
do EKS pelo Terraform).

---

## Chart Helm `wrench-api-k8s/`

```
wrench-api-k8s/
├── Chart.yaml
├── values.yaml
└── templates/
    ├── serviceaccount.yaml
    ├── secret.yaml
    ├── api/
    │   ├── deployment.yaml
    │   ├── service.yaml
    │   ├── hpa.yaml
    │   ├── http-route.yaml
    │   ├── https-route.yaml
    │   └── target-group.yaml
    └── tests/test-connection.yaml
```

| Template | Restrição |
|---|---|
| `serviceaccount.yaml` | A anotação `eks.amazonaws.com/role-arn` só é gerada com `serviceAccount.roleArn` preenchido; o EKS então injeta `AWS_ROLE_ARN` e `AWS_WEB_IDENTITY_TOKEN_FILE` nos pods |
| `secret.yaml` | `ConnectionStrings__Database` sobrescreve `ConnectionStrings:Database` do `appsettings` (o `__` mapeia a seção aninhada); o template falha sem `jwt.signingKey` ou `application.admin.password` |
| `api/hpa.yaml` | Renderizado só com `api.autoscaling.enabled` |
| `api/http-route.yaml` | Redirect 301 para HTTPS no listener `http` do Gateway de plataforma |
| `api/https-route.yaml` | Encaminha o hostname ao Service no listener `https` do Gateway de plataforma |
| `tests/test-connection.yaml` | `helm test`: `curl` no `/health/ready` do Service |

### Valores (`values.yaml`)

| Chave | Default | Observação |
|-------|---------|------------|
| `namespace` | `production` | Namespace de todos os recursos; o pipeline define `homologacao` ou `production` |
| `api.image.repository` | `...ecr.../wrench/api` | Sobrescrito no CI com o repositório ECR |
| `api.image.tag` | `latest` | Sobrescrito com a versão (tag semver) no CI |
| `api.autoscaling` | `enabled: true`, 1–6, 50% CPU | Controla o HPA |
| `gateway.name` | `bgt3-gw` | Gateway de plataforma do `infra-k8s` |
| `gateway.namespace` | `gateway` | Namespace do Gateway de plataforma |
| `httpRoute.hostname` | `api.bgt3.com.br` | Hostname do ambiente; precisa estar no certificado ACM e no DNS do `infra-k8s` |
| `database.host` | `prod-db.bgt3.com.br` | CNAME do RDS |
| `database.name` | `wrench_auto_repair` | Database do ambiente; homologação usa `wrench_auto_repair_hml` |
| `database.user` / `password` | placeholder | Credenciais do role da aplicação criado pelo `infra-db`, nunca o usuário master; sobrescritos no CI |
| `jwt.issuer` / `jwt.audience` | `Wrench Auto Repair` | Não são segredo, mas precisam ser idênticos aos do `lambda-auth`: a API só aceita token emitido com os mesmos valores |
| `jwt.signingKey` | `''` | Obrigatório; injetado no CI a partir do secret `JWT_SIGNING_KEY` |
| `application.admin.email` / `password` | `admin@wrench.com.br` / `''` | Administrador semeado na primeira subida; a senha é obrigatória e vem do secret `ADMIN_PASSWORD` |
| `application.credentials.aws.region` | `us-east-1` | Região do SDK AWS; as credenciais vêm da role IRSA |
| `serviceAccount.roleArn` | `''` | ARN da role IRSA do SES (injetado no CI) |
| `newRelic.licenseKey` | `''` | Vazio mantém a aplicação no ar e faz o New Relic recusar a exportação; injetado a partir de `NEW_RELIC_LICENSE_KEY` |

---

## Como aplicar

### Opção A — Chart Helm (recomendado, igual ao CI)

```bash
aws eks update-kubeconfig --region us-east-1 --name <CLUSTER_NAME>

helm upgrade --install wrench-hml ./wrench-api-k8s \
  -n homologacao --create-namespace \
  --set namespace=homologacao \
  --set httpRoute.hostname=hml-api.bgt3.com.br \
  --set api.image.repository=<ECR_API_REPO> \
  --set api.image.tag=<VERSION> \
  --set database.host=<DATABASE_HOSTNAME> \
  --set database.name=wrench_auto_repair_hml \
  --set database.user=<APP_DB_USER> \
  --set database.password=<APP_DB_PASS> \
  --set jwt.signingKey=<JWT_SIGNING_KEY> \
  --set application.admin.password=<ADMIN_PASSWORD> \
  --set serviceAccount.roleArn=<SES_IRSA_ROLE_ARN>

kubectl -n homologacao rollout status deployment/wrench-api-deployment --timeout=10m
helm test wrench-hml -n homologacao
```

Para produção, use o release `wrench`, o namespace `production`, o hostname `api.bgt3.com.br` e o
database `wrench_auto_repair`.

### Opção B — Manifestos crus (`kubectl`)

```bash
kubectl apply -f kubernetes/reference/wrench-production-namespace.yaml
kubectl apply -f kubernetes/reference/
kubectl -n production get pods,svc,hpa,httproute
```

Os manifestos crus representam o ambiente de produção e dependem do Gateway de plataforma já
criado pelo `infra-k8s`.

## Verificação rápida

```bash
kubectl -n production get pods
kubectl -n production get hpa wrench-api-hpa
kubectl -n production get httproute wrench-http-route \
  -o jsonpath='{.status.parents[0].conditions[?(@.type=="Accepted")].status}'
kubectl -n gateway get gateway bgt3-gw -o jsonpath='{.status.addresses[0].value}'
```

## Remover um ambiente

O workflow `destroy.yml` executa o equivalente a:

```bash
helm uninstall wrench-hml -n homologacao --wait
kubectl delete namespace homologacao --ignore-not-found
```

## Pré-requisitos (fornecidos pelos repositórios de infraestrutura)

Do **`infra-k8s`**:

- Cluster EKS com **Metrics Server** (HPA), **EBS CSI** (volumes), **AWS Load Balancer
  Controller** (ALB) e **CRDs da Gateway API** + `GatewayClass aws-lb-alb`.
- Gateway de plataforma `gateway/bgt3-gw`, ALB e certificado **ACM** com os dois hostnames.
- CNAMEs `api.bgt3.com.br` e `hml-api.bgt3.com.br` para o ALB.
- Role **IRSA do SES** para o envio de e-mail (stack `email/`).
- Repositórios **ECR** da imagem e do chart (stack `ecr/`).

Do **`infra-db`**:

- Instância **RDS PostgreSQL**, o CNAME `prod-db` e os databases `wrench_auto_repair` e
  `wrench_auto_repair_hml`.
- **Role de menor privilégio** da aplicação no Postgres, dono dos dois databases (stack `roles/`).
