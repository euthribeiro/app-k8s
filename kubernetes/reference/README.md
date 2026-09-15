# Kubernetes — Manifestos e deploy da aplicação

Este documento descreve os recursos Kubernetes da aplicação _Wrench Auto Repair_
e como aplicá-los. Há **dois caminhos**:

- **`kubernetes/reference/`** — manifestos YAML "crus", usados como **referência** de leitura e
  para aplicação manual/local (`kubectl apply`). Atendem ao requisito de _"criar manifestos YAML
  para deploy em Kubernetes"_ da Fase 2.
- **`wrench-api-k8s/`** — **chart Helm** (caminho de deploy real, usado pelo CI/CD). Os mesmos
  recursos, parametrizados via `values.yaml` e injetados pela pipeline.

> O banco de dados é o **RDS gerenciado**, provisionado pelo repositório `infra-db`. Os manifestos
> de Postgres in-cluster que existiam na Fase 2 (StatefulSet, PVC, Service do banco, StorageClass)
> foram **removidos** na Fase 3: não há mais banco dentro do cluster, nem comentado.

---

## Recursos criados (namespace `production`)

| Recurso | Kind | Papel |
|---------|------|-------|
| `production` | `Namespace` | Namespace de todos os recursos da aplicação |
| `wrench-api-sa` | `ServiceAccount` | SA da API; anotada com a role IRSA do SES (envio de e-mail sem chave estática) |
| `db-credentials` | `Secret` | Connection string do banco + config da app (ambiente, admin, região AWS) |
| `wrench-api-deployment` | `Deployment` | API .NET, porta 8080, RollingUpdate, probes `/health` e `/health/ready` |
| `wrench-api-svc` | `Service` (ClusterIP) | Expõe a API na porta 8080 dentro do cluster |
| `wrench-api-hpa` | `HorizontalPodAutoscaler` | Escala a API por CPU (50%), 1–6 réplicas |
| `bgt3-gw` | `Gateway` (Gateway API) | Entrada L7; listeners HTTP:80 e HTTPS:443 |
| `bgt3-gw-lbconfig` | `LoadBalancerConfiguration` (CRD AWS) | Configura o ALB: `internet-facing` + certificado ACM no 443 |
| `wrench-api-tg` | `TargetGroupConfiguration` (CRD AWS) | `targetType: ip` para o target group do ALB |
| `wrench-http-route` | `HTTPRoute` | Roteia o hostname para o `wrench-api-svc` (listener HTTPS) |
| `wrench-https-redirect` | `HTTPRoute` | Redirect 301 HTTP → HTTPS (só quando há certificado) |

O ALB em si **não** é um recurso do YAML: é criado pela AWS quando o **AWS Load
Balancer Controller** observa o `Gateway`. A `GatewayClass` `aws-lb-alb` e os CRDs
da Gateway API são pré-requisitos criados pelo Terraform (stack `infra/`).

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
├── Chart.yaml                 # name: wrench-api, version 0.4.0
├── values.yaml                # parâmetros (imagem, gateway, banco, SES…)
└── templates/
    ├── namespace.yaml
    ├── serviceaccount.yaml     # IRSA quando serviceAccount.roleArn é setado
    ├── secret.yaml             # ConnectionStrings__Database + config da app
    ├── gateway.yaml            # listener HTTPS só quando gateway.certificateArn != ''
    ├── loadbalancerconfiguration.yaml
    ├── api/
    │   ├── deployment.yaml
    │   ├── service.yaml
    │   ├── hpa.yaml            # renderizado só se api.autoscaling.enabled
    │   ├── http-route.yaml     # redirect HTTP→HTTPS (condicional)
    │   ├── https-route.yaml
    │   └── target-group.yaml
    └── tests/test-connection.yaml   # `helm test`: curl no /health/ready
```

### Valores principais (`values.yaml`)

| Chave | Default | Observação |
|-------|---------|------------|
| `api.image.repository` | `...ecr.../wrench/api` | Sobrescrito no CI com o repo ECR |
| `api.image.tag` | `latest` | Sobrescrito com a versão (tag semver) no CI |
| `api.autoscaling` | `enabled: true`, 1–6, 50% CPU | Controla o HPA |
| `gateway.className` | `aws-lb-alb` | Casa com a `GatewayClass` do Terraform |
| `gateway.scheme` | `internet-facing` | Default do LBC é `internal` — daí ser explícito |
| `gateway.certificateArn` | `''` | ARN do ACM; vazio = só HTTP |
| `httpRoute.hostname` | `api.bgt3.com.br` | Deve casar com o ACM/DNS |
| `database.host` | `prod-db.bgt3.com.br` | CNAME do RDS |
| `database.user` / `password` | placeholder | Sobrescritos no CI pelas credenciais do role da app |
| `application.admin.password` | `''` | Obrigatório; injetado no CI a partir do secret `ADMIN_PASSWORD` |
| `jwt.issuer` / `jwt.audience` | `Wrench Auto Repair` | Devem ser idênticos aos do `lambda-auth` |
| `jwt.signingKey` | `''` | Obrigatório; injetado no CI a partir do secret `JWT_SIGNING_KEY` |
| `serviceAccount.roleArn` | `''` | ARN da role IRSA do SES (injetado no CI) |

---

## Como aplicar

### Opção A — Chart Helm (recomendado, igual ao CI)

```bash
# 1. Autenticar no cluster
aws eks update-kubeconfig --region us-east-1 \
  --name "$(terraform -chdir=terraform/infra output -raw cluster_name)"

# 2. Instalar/atualizar a aplicação
helm upgrade --install wrench ./wrench-api-k8s \
  -n production --create-namespace \
  --set api.image.repository=<ECR_API_REPO> \
  --set api.image.tag=<VERSION> \
  --set gateway.certificateArn="$(terraform -chdir=terraform/infra output -raw acm_certificate_arn)" \
  --set database.host="$(terraform -chdir=terraform/infra output -raw database_hostname)" \
  --set database.name="$(terraform -chdir=terraform/infra output -raw database_name)" \
  --set database.user=<APP_DB_USER> \
  --set database.password=<APP_DB_PASS> \
  --set serviceAccount.roleArn="$(terraform -chdir=terraform/email output -raw app_ses_irsa_role_arn)"

# 3. Esperar o ALB do Gateway ficar pronto
kubectl -n production wait --for=condition=Programmed gateway/bgt3-gw --timeout=10m

# 4. (opcional) Testar
helm test wrench -n production
```

### Opção B — Manifestos crus (`kubectl`)

```bash
kubectl apply -f kubernetes/wrench-production-namespace.yaml
kubectl apply -f kubernetes/          # aplica os manifestos ativos do diretório
kubectl -n production get pods,svc,hpa,gateway,httproute
```

> Antes de aplicar os manifestos crus, ajuste o `defaultCertificate` em
> `wrench-lb-config.yaml` (placeholder `REPLACE-WITH-REAL-ARN`) com o ARN real do
> certificado ACM.

## Verificação rápida

```bash
kubectl -n production get pods                 # pods da API rodando
kubectl -n production get hpa wrench-api-hpa    # métricas do autoscaler
kubectl -n production get gateway bgt3-gw \
  -o jsonpath='{.status.addresses[0].value}'   # hostname do ALB
```

## Pré-requisitos (fornecidos pelos repositórios de infraestrutura)

Do **`infra-k8s`**:

- Cluster EKS com **Metrics Server** (HPA), **EBS CSI** (volumes), **AWS Load Balancer
  Controller** (ALB) e **CRDs da Gateway API** + `GatewayClass aws-lb-alb` (stack `infra/`).
- Certificado **ACM** validado, casando com `httpRoute.hostname` (stack `infra/`).
- Role **IRSA do SES** para o envio de e-mail (stack `email/`).
- Repositórios **ECR** da imagem e do chart (stack `ecr/`).

Do **`infra-db`**:

- Instância **RDS PostgreSQL** e o CNAME `prod-db` (stack `rds/`).
- **Role de menor privilégio** da aplicação no Postgres (stack `roles/`).
