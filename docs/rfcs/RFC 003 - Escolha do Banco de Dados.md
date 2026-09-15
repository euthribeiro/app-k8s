# RFC 003 — Escolha do Banco de Dados

| | |
|---|---|
| **Status** | Aprovado |
| **Autor** | Grupo BGT³ |
| **Revisores** | Bruno Barreto, João Paulo Seixas, Thiago Ribeiro |
| **Data** | Setembro de 2026 |
| **Fase** | Tech Challenge — Fase 3 |
| **Substitui** | — |
| **Relacionado** | ADR 001 (PostgreSQL), ADR 002 (RDS e privilégios), RFC 001 (nuvem), RFC 002 (autenticação), ADR 004 (ambientes) |

## Resumo

Este RFC registra a escolha do **PostgreSQL 18 gerenciado pelo Amazon RDS** como banco de dados do
Wrench Auto Repair, compara-o com as alternativas viáveis na AWS e explicita as questões que
continuam abertas. A justificativa detalhada do modelo relacional, com o diagrama ER e a explicação
dos relacionamentos, está em [Modelo relacional](../database/modelo-relacional.md).

## Contexto e motivação

O banco atende a dois consumidores com perfis de acesso diferentes:

* a **API no EKS**, dona do schema, que escreve e lê os quatro bounded contexts (autenticação,
  cadastro, estoque e ordem de serviço) por Entity Framework Core com migrations;
* a **Lambda de autenticação**, que só lê três tabelas para validar existência e status do cliente
  antes de emitir o JWT.

O domínio é transacional por natureza. Abrir uma ordem de serviço, registrar o diagnóstico com as
peças do orçamento e aprovar ou recusar esse orçamento são operações que precisam ser atômicas. As
consultas de operação e de monitoramento filtram e agregam por status e por intervalo de datas —
listagem de ordens ativas, tempo médio por fase, volume diário.

Com a expansão para várias unidades, a escolha precisa suportar crescimento de volume sem trocar de
motor, ser provisionada inteiramente por Terraform e permitir segregar privilégios entre os dois
consumidores.

## Critérios de decisão

1. **Consistência transacional** — ACID com múltiplas tabelas na mesma transação e integridade por
   constraint (PK, FK, unique), não por código.
2. **Aderência ao modelo** — entidades com relacionamentos estáveis e consultas de agregação por
   status e datas.
3. **Integração com o stack** — provider maduro para EF Core 10, migrations versionadas e execução
   local idêntica em contêiner.
4. **Operação gerenciada na AWS via Terraform** — backup, patch e monitoramento sem trabalho manual.
5. **Menor privilégio granular** — roles distintos por consumidor, com concessão por coluna.
6. **Custo** — instância mínima viável dentro do orçamento acadêmico, sem licença.

## Alternativas avaliadas

### PostgreSQL no Amazon RDS — escolhida

Atende a todos os critérios. O provider `Npgsql.EntityFrameworkCore.PostgreSQL` é o de maior
maturidade do ecossistema .NET fora do SQL Server, os tipos nativos (`uuid`, `timestamptz`,
`numeric`) mapeiam diretamente o domínio, e funções SQL declaradas pela aplicação
(`datediff_milliseconds`) viabilizam as agregações de tempo por fase no próprio banco. O modelo de
privilégios do PostgreSQL permite `GRANT SELECT` por coluna, o que restringe a Lambda às colunas do
contrato sem criar objetos adicionais. O RDS entrega backup automático, patch e métricas, e o
provider `cyrilgdn/postgresql` provisiona os roles pelo Terraform.

### MySQL no Amazon RDS

Também é relacional, gerenciado e de baixo custo. Perdeu por aderência: tipos de data com fuso e
UUID nativo são menos diretos, o provider EF Core de uso corrente é mantido pela comunidade
(Pomelo) e o time já tinha o schema e as migrations da Fase 1 escritos contra o PostgreSQL. Trocar
seria reescrever migrations sem ganho funcional.

### SQL Server no Amazon RDS

Tem a melhor integração com EF Core. Perdeu por custo: exige licença incluída na instância, a
menor classe suportada é maior que a `db.t4g.micro`, e não há contrapartida funcional para o
domínio da oficina.

### Amazon Aurora PostgreSQL

Mantém compatibilidade total com o PostgreSQL e acrescenta failover rápido, réplicas de leitura e
armazenamento distribuído. Perdeu por custo mínimo: o cluster provisionado parte de classes
maiores, e o Aurora Serverless v2 cobra capacidade mínima contínua. Como a compatibilidade é de
protocolo e de SQL, a migração futura para Aurora não exige mudança na aplicação nem na Lambda —
fica registrada como evolução quando disponibilidade multi-AZ se tornar requisito.

### Amazon DynamoDB

Serverless, com latência previsível e integração natural com Lambda. Perdeu por aderência ao
domínio: transações entre entidades, unicidade de documento, e-mail e placa, consultas por status
com ordenação por data e agregações de tempo médio exigiriam índices secundários desenhados por
padrão de acesso, streams para projeções e código para manter invariantes que o banco relacional
garante por constraint. O EF Core não tem provider oficial de primeira linha para DynamoDB.

## Comparação

| Critério | RDS PostgreSQL | RDS MySQL | RDS SQL Server | Aurora PostgreSQL | DynamoDB |
|---|---|---|---|---|---|
| ACID entre tabelas | sim | sim | sim | sim | limitado |
| Integridade por constraint | **completa** | completa | completa | completa | não |
| Agregação por status e datas | **nativa** | nativa | nativa | nativa | projeções |
| Provider EF Core | **maduro** | comunidade | oficial | maduro | ausente |
| GRANT por coluna | **sim** | sim | sim | sim | IAM por item |
| Terraform + gerenciado | sim | sim | sim | sim | sim |
| Custo mínimo | **baixo** | baixo | alto | médio | baixo |
| Licença | não | não | sim | não | não |

## Decisão

**Adotar o PostgreSQL 18 no Amazon RDS**, provisionado pelo repositório `infra-db`, com:

| Aspecto | Definição |
|---|---|
| Instância | `db.t4g.micro`, 20 GB, backup de 7 dias |
| Schema | `public`, criado e evoluído pelas migrations EF Core da API |
| Role da aplicação | CRUD e `CREATE` no schema; dono das tabelas |
| Role da Lambda | `SELECT` restrito às colunas `Clientes(Id, Documento, Email)`, `Usuarios(Id, Email, PerfilId, Ativo)` e `Perfis(Id, Nome)` |
| Usuário master | usado apenas pelo Terraform para criar os roles |
| Contrato com a Lambda | colunas acima, verificadas por teste de contrato no `app-k8s` |
| Ambientes | homologação e produção compartilham a instância (ADR 004) |

## Impacto

**Na aplicação.** Nenhum: o modelo e as migrations existentes permanecem. O teste de contrato passa
a falhar o pipeline da API se uma migration alterar uma coluna lida pela Lambda.

**Na Lambda.** O `DbContext` da Lambda mapeia só as colunas do contrato e nunca executa migrations.
A consulta de autenticação percorre os índices únicos de `Clientes.Documento` e `Usuarios.Email` e
a PK de `Perfis`.

**Na infraestrutura.** O stack `roles` do `infra-db` concede os privilégios dos dois consumidores.
Concessão por coluna só é aceita quando as tabelas já existem, então no primeiro provisionamento o
`roles` é reaplicado depois do primeiro deploy da API.

**Na reversibilidade.** Aurora PostgreSQL é o caminho de evolução sem reescrita. Qualquer outro
motor implicaria reescrever migrations, o `DbContext` da Lambda e o teste de contrato.

## Questões em aberto

1. **Instância única e single-AZ.** O backup garante recuperação, não continuidade. Multi-AZ ou
   Aurora dobram o custo mínimo.
2. **Acesso público.** `publicly_accessible = true` é exigido pela execução remota do HCP Terraform
   para provisionar os roles e pela Lambda fora de VPC. A exposição é mitigada por TLS obrigatório
   e senha forte, mas o caminho correto em produção é Lambda em VPC com runner privado.
3. **Banco compartilhado entre homologação e produção.** Testes de carga em homologação afetam
   produção (ADR 004).
4. **Tipos monetários heterogêneos.** `Pecas.Valor` é `double precision`, `OrdemServico.ValorServico`
   é `numeric` sem escala e `OrdemServicoItem.ValorUnitario` é `decimal(18,2)`. Ver as recomendações
   do modelo relacional.
5. **Rotação de credenciais.** Senhas dos roles e chave de assinatura do JWT não têm rotação
   automática.

## Referências

* [Modelo relacional e diagrama ER](../database/modelo-relacional.md)
* [ADR 001 — Escolha do Banco de Dados](../adrs/ADR%20001%20-%20Escolha%20do%20Banco%20de%20Dados.md)
* [ADR 002 — Migração para AWS RDS PostgreSQL e Segregação de Privilégios](../adrs/ADR%20002%20-%20Escolha%20do%20Banco%20de%20Dados.md)
* [RFC 001 — Escolha do Provedor de Nuvem](./RFC%20001%20-%20Escolha%20da%20Nuvem.md)
* [ADR 004 — Uso de HorizontalPodAutoscaler e Segregação de Ambientes](../adrs/ADR%20004%20-%20Uso%20de%20HPA.md)
