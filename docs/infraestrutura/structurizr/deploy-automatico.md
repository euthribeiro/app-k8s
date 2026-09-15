# Structurizr — deploy automático (Terraform + Helm + GitHub Actions)

Sobe o **Structurizr Lite** (público, view-only) no EKS via **Cloudflare Tunnel**,
no mesmo padrão do resto do projeto: **Terraform** provisiona o túnel/DNS, um
**chart Helm** empacota os manifests e o **GitHub Actions** orquestra. Push que
altere o DSL/chart/stack → o ambiente atualiza sozinho.

## Peças

```
terraform/structurizr/     # Cloudflare Tunnel + ingress + CNAME + output do token
structurizr-k8s/           # chart Helm (Lite view-only + cloudflared + configmap do DSL)
.github/workflows/
  structurizr.yml          # orquestrador (push nos paths do structurizr / manual)
  structurizr-infra.yml    # reusable: terraform apply (túnel/DNS)
  structurizr-deploy.yml   # reusable: helm upgrade --install (lê o token do state)
docs/infraestrutura/structurizr/workspace.dsl   # fonte única da verdade do modelo C4
```

## Fluxo

```
push (master) nos paths do structurizr
        │
        ▼
  structurizr-infra  → terraform apply em terraform/structurizr
        │              (cria o tunnel, o ingress e o CNAME; expõe o token no state)
        ▼
  structurizr-deploy → cp workspace.dsl -> chart/files
                       terraform output -raw tunnel_token  (mascarado)
                       helm upgrade --install structurizr ./structurizr-k8s
                         --set tunnelToken=<token> --set hostname=<host>
```

O token do conector **nunca** é commitado nem passado como input entre jobs: o
job de deploy o lê direto do state (HCP) e injeta no chart como Secret.

## Pré-requisitos (uma vez)

### 1. Workspace HCP

Criar o workspace **`wrench_auto_repair_structurizr`** (working directory
`terraform/structurizr`) e definir como *Terraform Variables* (sensíveis):

- `cloudflare_api_token` — **token de conta** com permissões **Account →
  Cloudflare Tunnel: Edit** e **Zone → DNS: Edit** (o token atual, só de DNS, não
  basta para criar o túnel).
- `cloudflare_account_id`
- `cloudflare_zone_id`

### 2. Secrets/variables do GitHub

Já existem no projeto (reaproveitados): secret `TF_API_TOKEN`, secrets
`AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY`, variable `AWS_REGION`.

Opcional: variable **`EKS_CLUSTER_NAME`** (o workflow usa `eks-wrench-auto-repair`
por padrão se ela não existir).

## Como dispara

- **Automático:** `git push` na `master` que toque em `terraform/structurizr/**`,
  `structurizr-k8s/**`, `docs/infraestrutura/structurizr/workspace.dsl` ou os
  workflows `structurizr*.yml`.
- **Manual:** aba Actions → *Structurizr (Tunnel + Deploy)* → *Run workflow*.

Depois do primeiro deploy, o link fica público em
**`https://structurizr-wrench-api.bgt3.com.br`**.

## Atualizar os diagramas

Só editar `docs/infraestrutura/structurizr/workspace.dsl` e dar push. O
`structurizr-deploy` copia o DSL para o chart, e o `checksum/workspace` no
Deployment força o pod a reiniciar já com a versão nova.

## View-only e concorrência (relembrando)

O Lite é single-user e editável; aqui o DSL vem de um ConfigMap read-only copiado
para um `emptyDir` efêmero, então **nada que editarem persiste**. Mantenha
`replicas: 1` no `structurizr-lite` (sem HPA). O `cloudflared` pode ter 2+.

## Relação com os manifests manuais

A pasta `kubernetes/structurizr/` tem os mesmos recursos em YAML cru, para
aplicar à mão (`kubectl apply`) sem a pipeline. O chart `structurizr-k8s/` é o
caminho **automatizado** e é o que a esteira usa — use um ou outro, não os dois
ao mesmo tempo no mesmo cluster.
