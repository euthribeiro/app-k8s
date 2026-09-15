# ADR 002 - Migração para AWS RDS PostgreSQL e Segregação de Privilégios

## Status

Aceito

## Contexto

A decisão anterior (ADR 001) estabeleceu o PostgreSQL como o motor de banco de dados relacional para o sistema da Wrench Auto Repair, devido à sua robustez, suporte transacional ACID e excelente integração com o Entity Framework Core. Nessa primeira etapa, o banco era executado localmente em container Docker.

Com o avanço do projeto para um ambiente produtivo na nuvem, novas necessidades operacionais surgiram:

* Reduzir a sobrecarga de gerenciamento de infraestrutura (backups, patches, alta disponibilidade e monitoramento).
* Garantir a segurança operacional seguindo o princípio do menor privilégio (*Least Privilege*), evitando que a aplicação execute queries utilizando as credenciais administrativas (*master user*) do banco de dados.
* Automatizar a criação de recursos e o controle de acessos através de esteiras de CI/CD via HCP Terraform (Terraform Cloud).

---

## Decisão

1. **Adotar o Amazon RDS para PostgreSQL 18** como a plataforma de banco de dados gerenciado, substituindo o container Docker usado em desenvolvimento.
2. **Utilizar o HCP Terraform (Terraform Cloud)** com execução remota para gerenciar o estado da infraestrutura de dados.
3. **Isolar as responsabilidades de privilégios:** o Terraform conecta-se ao RDS utilizando o usuário administrativo (`rds_master_username`) apenas para provisionar, de forma estrita, um novo papel/role específico para a aplicação (`application_database_username`), que será a proprietária e operadora das tabelas via Entity Framework Migrations.

---

## Alternativas Consideradas

* **PostgreSQL em Instância EC2 (Self-hosted):** daria controle total sobre as configurações do sistema operacional, mas manteria a desvantagem já listada na ADR 001 sobre o peso do gerenciamento operacional (configurar replicação, rotinas de backup e janelas de manutenção manualmente).
* **Manter credenciais Master na Aplicação:** utilizar o usuário padrão do RDS diretamente na API .NET simplificaria o Terraform, mas violaria severamente as práticas recomendadas de segurança cibernética (um eventual ataque de SQL Injection comprometeria todo o cluster do banco de dados).

---

## Detalhes da Implementação (Baseado na IaC)

O ecossistema foi dividido em duas camadas de estado via Terraform:

* **Camada de Infraestrutura (`wrench_auto_repair`):** onde o recurso físico real (`aws_db_instance`) reside e expõe os outputs de conexão (endpoint e nome do banco). O endpoint público do RDS é ainda referenciado por um CNAME `prod-db` no Cloudflare.
* **Camada de Dados (`wrench_auto_repair_postgres`):** consome o estado remoto da infraestrutura e utiliza o provider especializado `cyrilgdn/postgresql` (`~> 1.25`) com o parâmetro de segurança `superuser = false` ativado — obrigatório, dado que o usuário master do RDS não é um superusuário PostgreSQL de verdade e, sem esse ajuste, o provider tentaria comandos que falhariam.

### Parâmetros provisionados

| Parâmetro | Valor |
| :--- | :--- |
| Engine | PostgreSQL 18 (`postgres`) |
| Classe da instância | `db.t4g.micro` |
| Armazenamento | 20 GB |
| Retenção de backups | 7 dias |
| Região | `us-east-1` |
| Acesso | `publicly_accessible = true` + Security Group liberado para a porta 5432 |

### Matriz de Privilégios Concedidos à Aplicação (`app`)

Como a estratégia de evolução de schema é *Code First* (via EF Core Migrations), o role da aplicação recebe acesso explícito para criar e manipular objetos dentro do schema `public`, eliminando a necessidade de diretivas de *Default Privileges*:

| Tipo de Objeto | Privilégios Atribuídos | Justificativa |
| :--- | :--- | :--- |
| **Database** | `CONNECT`, `CREATE` | Permitir conexão inicial e criação de extensões/schemas se necessário. |
| **Schema (public)** | `USAGE`, `CREATE` | Necessário para o EF Core criar e alterar tabelas de Migrations. |
| **Tables** | `SELECT`, `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE`, `REFERENCES`, `TRIGGER` | Operações completas de CRUD, gerenciamento de chaves estrangeiras e triggers de auditoria. |
| **Sequences** | `USAGE`, `SELECT`, `UPDATE` | Permitir o incremento correto de IDs auto-gerados (`Identity` do .NET). |

---

## Consequências

### Positivas

* **Segurança Reforçada:** a API do backend roda sob um usuário com escopo limitado, mitigando riscos de segurança severos no banco de dados.
* **Segurança de Logs:** as credenciais críticas do banco de dados (tanto master quanto do app) e os outputs sensíveis são marcados explicitamente como ocultos (`sensitive = true`), impedindo vazamentos em logs de CI/CD ou CLI.
* **Resiliência e Escalabilidade:** delegação de backups automatizados, correções de segurança (*patching*) e monitoramento para a infraestrutura gerenciada da AWS (RDS).

### Negativas

* **Complexidade na Execução Remota:** como as instâncias do Terraform rodam nos servidores da HCP Terraform, a porta do banco de dados precisa estar acessível de forma pública (`publicly_accessible = true` + SG liberado) para que o provider consiga injetar as permissões de acesso.
* **Dependência de State Sharing:** o workspace de dados depende estritamente do sucesso e do compartilhamento de estado do workspace de infraestrutura básica.
* **Divergência de versão entre ambientes:** o desenvolvimento local utiliza PostgreSQL em container (versão 16), enquanto a produção roda PostgreSQL 18 no RDS. É recomendável alinhar as versões para evitar diferenças de comportamento.

---

## Notas Adicionais

* As senhas e usuários administrativos do RDS devem ser injetados estritamente como variáveis de ambiente secretas protegidas no painel do HCP Terraform ou em gerenciadores de segredos corporativos (ex.: AWS Secrets Manager / CI Secrets), nunca versionadas no código-fonte.
