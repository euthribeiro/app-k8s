# RFC 001 — Escolha do Provedor de Nuvem

| | |
|---|---|
| **Status** | Aprovado |
| **Autor** | Thiago Ribeiro |
| **Revisores** | Bruno Barreto, João Paulo Seixas |
| **Data** | Agosto de 2026 |
| **Fase** | Tech Challenge — Fase 3 |
| **Substitui** | — |
| **Relacionado** | ADR 002 (RDS), ADR 005 (comunicação), ADR 004 (HPA) |

## Resumo

Este RFC registra formalmente a escolha da **AWS** como provedor de nuvem do Wrench Auto Repair, os critérios que levaram a ela, as alternativas avaliadas e o custo de reversão. A decisão foi tomada na Fase 2 e é reafirmada aqui com o benefício de um ciclo de operação real — o que permitiu avaliá-la contra experiência concreta, e não só contra expectativa.

## Contexto e motivação

A Fase 2 levou a aplicação de execução local para um ambiente produtivo. Isso exigiu, de uma vez: cluster Kubernetes gerenciado com autoescala, banco relacional gerenciado, registro de imagens, certificado TLS, e-mail transacional, gerenciamento de identidade para cargas de trabalho e um backend de estado para o Terraform.

A Fase 3 acrescenta função serverless e API Gateway, e a exigência de que **tudo seja provisionado por infraestrutura como código**, distribuído em quatro repositórios com CI/CD independente.

A escolha precisava ser tomada uma vez e sustentar as duas fases, porque migrar de provedor no meio custaria mais do que o projeto inteiro.

## Critérios de decisão

Os critérios foram ponderados na seguinte ordem, decidida antes de olhar para qualquer provedor:

1. **Cobertura de serviços gerenciados** — cada serviço que não fosse gerenciado viraria trabalho operacional do grupo, que tem três pessoas e prazo de uma fase.
2. **Maturidade do provider Terraform** — todo o provisionamento é IaC; um provider incompleto significaria recursos criados na mão, fora do state.
3. **Identidade para cargas de trabalho sem credencial estática** — a aplicação precisa enviar e-mail; fazer isso com access key em secret é dívida de segurança desde o primeiro dia.
4. **Custo dentro do orçamento acadêmico** — instâncias pequenas viáveis e camada gratuita relevante.
5. **Familiaridade da equipe** — tempo gasto aprendendo console é tempo não gasto no domínio.
6. **Integração com a plataforma de observabilidade** a ser escolhida na Fase 3.

## Alternativas avaliadas

### AWS — escolhida

Cobre todos os serviços exigidos com equivalentes de primeira linha: EKS, RDS PostgreSQL, ECR, ACM, SES, Lambda e API Gateway. O provider `hashicorp/aws` é o mais maduro do ecossistema Terraform, e o **IRSA** (IAM Roles for Service Accounts) resolve o critério 3 de forma nativa — a `ServiceAccount` do pod assume uma role IAM via OIDC, sem nenhuma credencial estática no cluster. É exatamente o mecanismo usado hoje pelo envio de e-mail via SES.

A equipe já tinha experiência prévia com a plataforma, o que valia mais do que qualquer vantagem marginal de outro provedor.

### Google Cloud

GKE é, tecnicamente, o Kubernetes gerenciado mais refinado dos três — o modo Autopilot elimina a gestão de node group, que é trabalho real no EKS. Cloud SQL e Artifact Registry cobrem banco e registry sem ressalva, e o Workload Identity é equivalente ao IRSA.

Perdeu por dois motivos. O primeiro é e-mail transacional: o GCP não tem serviço nativo equivalente ao SES, e a recomendação da própria documentação é usar um terceiro (SendGrid, Mailgun), o que traria um provedor adicional, uma conta a mais e um ponto de integração fora da IaC principal. O segundo é familiaridade: nenhum dos três integrantes tinha experiência de operação em GCP.

### Microsoft Azure

AKS, Azure Database for PostgreSQL e Container Registry cobrem o núcleo, e a integração com Entra ID é forte — relevante se o cliente fosse uma organização já dentro do ecossistema Microsoft, o que não é o caso.

Perdeu porque o provider `azurerm` exige mais recursos intermediários para chegar ao mesmo resultado (resource groups, service principals, atribuições de papel explícitas), aumentando a superfície de IaC sem ganho correspondente. E o Azure Communication Services, equivalente ao SES, é mais recente e menos rodado.

### Multi-cloud ou agnóstico

Foi considerada e descartada rapidamente. Manter a aplicação portável entre provedores significaria abrir mão justamente dos serviços gerenciados que sustentam o critério 1, reintroduzindo operação manual de banco, e-mail e identidade. É o oposto do objetivo declarado do desafio, que é elevar a aplicação a um nível de operação corporativa.

## Comparação

| Critério | AWS | GCP | Azure |
|---|---|---|---|
| Kubernetes gerenciado | EKS | GKE (mais refinado) | AKS |
| PostgreSQL gerenciado | RDS | Cloud SQL | Azure Database |
| Registro de imagens | ECR | Artifact Registry | ACR |
| Certificado TLS | ACM (gratuito) | Certificate Manager | App Service Certificates |
| E-mail transacional | **SES (nativo)** | ausente — exige terceiro | Communication Services |
| Serverless | Lambda | Cloud Functions | Functions |
| API Gateway | API Gateway | API Gateway / Apigee | API Management |
| Identidade sem credencial | **IRSA** | Workload Identity | Workload Identity |
| Maturidade do provider Terraform | **alta** | alta | média-alta |
| Familiaridade da equipe | **sim** | não | parcial |

## Decisão

**Adotar a AWS**, na região `us-east-1`, com:

| Necessidade | Serviço |
|---|---|
| Kubernetes com autoescala | Amazon EKS + HPA |
| Banco relacional gerenciado | Amazon RDS PostgreSQL 18 |
| Registro de imagens e charts | Amazon ECR (incl. charts OCI) |
| Certificado TLS | AWS Certificate Manager |
| E-mail transacional | Amazon SES, acessado via IRSA |
| Função serverless | AWS Lambda |
| Borda autenticada | Amazon API Gateway |
| Balanceamento L7 | ALB via AWS Load Balancer Controller |
| Estado do Terraform | HCP Terraform, organização `bgt3` |

**DNS fica no Cloudflare**, fora da AWS. O domínio já estava registrado lá, e o Cloudflare também serve o túnel que expõe o Structurizr. Route 53 traria integração mais direta com o ACM, mas migrar zona DNS no meio do projeto é risco sem retorno.

## Impacto

**Na arquitetura.** A escolha se materializa em pontos que não são triviais de trocar: a role IRSA para o SES, a descoberta de certificado ACM por hostname no Gateway API, os CRDs do AWS Load Balancer Controller e a integração da Lambda com o API Gateway. Cada um desses é uma amarra real com a AWS.

**Nos repositórios.** Os quatro repositórios usam o provider `hashicorp/aws` com backend no HCP Terraform. Trocar de provedor implicaria reescrever `infra-k8s`, `infra-db` e o Terraform do `lambda-auth` — a aplicação .NET em si migraria quase sem alteração.

**No custo.** O dimensionamento é o mínimo viável: `db.t4g.micro` com 20 GB, node group pequeno, HPA partindo de uma réplica. O ALB e o NAT Gateway são os itens de custo fixo que não escalam para zero.

**Na reversibilidade.** A camada de aplicação é portável — é .NET em contêiner, com Helm, falando PostgreSQL. O que não é portável é a IaC e os mecanismos de identidade. Uma migração hipotética custaria a reescrita da infraestrutura, não da aplicação. Isso é aceitável e foi decidido conscientemente ao rejeitar a alternativa agnóstica.

## Questões em aberto

1. **Região única.** Tudo roda em `us-east-1`, sem qualquer redundância regional. Uma indisponibilidade da região derruba o sistema inteiro. Não é problema para o escopo atual, mas deveria ser reavaliado se o requisito de disponibilidade endurecer.
2. **RDS single-AZ.** `backup_retention_period = 7` dá recuperação, não continuidade. Multi-AZ dobraria o custo da instância.
3. **`skip_final_snapshot = true`.** Conveniente no ciclo de criar e destruir do projeto, perigoso em operação real — um `terraform destroy` acidental é irreversível. Ver o procedimento de migração de state em `infra-db/docs/migracao-de-state.md`.
4. **Plataforma de observabilidade.** Datadog e New Relic são ambos SaaS externos e neutros quanto ao provedor; a escolha entre eles é da frente de observabilidade e não altera este RFC. CloudWatch nativo foi considerado insuficiente para os dashboards exigidos pelo enunciado.

## Referências

* [ADR 002 — Migração para AWS RDS PostgreSQL e Segregação de Privilégios](../adrs/ADR%20002%20-%20Escolha%20do%20Banco%20de%20Dados.md)
* [ADR 005 — Padrão de Comunicação entre Componentes](../adrs/ADR%20005%20-%20Padrao%20de%20Comunicacao.md)
* [ADR 004 — Uso de HorizontalPodAutoscaler e Segregação de Ambientes](../adrs/ADR%20004%20-%20Uso%20de%20HPA.md)
* [Diagrama de componentes](../infraestrutura/arquitetura.md)
