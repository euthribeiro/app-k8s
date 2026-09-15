# ADR 001 - Escolha do Banco de Dados

## Status

Aceito

## Contexto

O sistema da Wrench Auto Repair exige persistência consistente e transacional para entidades centrais do domínio, como clientes, veículos, ordens de serviço, peças e usuários.

Além disso, o sistema possui requisitos relevantes:

* Integridade referencial forte (ex: relacionamento entre ordem de serviço e itens)
* Suporte a transações ACID (ex: abertura/fechamento de ordens)
* Evolução controlada de schema (migrações com versionamento)
* Integração com Entity Framework Core e PostgreSQL
* Possível crescimento futuro com necessidade de performance e escalabilidade

Dado esse cenário, a escolha do banco de dados precisa privilegiar consistência, modelagem relacional e previsibilidade operacional.

---

## Decisão

Adotar o PostgreSQL como banco de dados principal do sistema.

---

## Alternativas Consideradas

### MongoDB

* Modelo orientado a documentos
* Flexível para schemas dinâmicos
* Não ideal para forte consistência relacional e transações complexas

### Oracle

* Robusto e altamente performático
* Licenciamento elevado e maior complexidade operacional

### SQL Server

* Excelente integração com stack Microsoft
* Pode gerar custo de licenciamento dependendo do ambiente
* Menor portabilidade em ambientes Linux comparado ao Postgres

### MySQL

* Popular e amplamente utilizado
* Menor aderência a recursos avançados (comparado ao Postgres) em cenários complexos
* Diferenças de comportamento em transações dependendo do engine (InnoDB vs outros)

---

## Consequências

### Positivas

* **Open-source e sem custo de licença**
  Reduz custo operacional e permite uso em qualquer ambiente (dev, staging, produção)

* **Aderência ao modelo relacional**
  Excelente suporte a integridade referencial, constraints e modelagem rica

* **Suporte completo a ACID**
  Essencial para garantir consistência em operações críticas (ex: ordens de serviço)

* **Integração madura com Entity Framework Core**
  Permite uso eficiente de migrations, LINQ e otimizações

* **Recursos avançados**

  * JSONB para dados semi-estruturados
  * Indexação avançada (GIN, GiST)
  * Full-text search
  * Window functions

* **Alta portabilidade e suporte a Linux**
  Ideal para ambientes containerizados (Docker/Kubernetes)

* **Comunidade ativa e documentação extensa**

---

### Negativas

* **Curva de aprendizado para features avançadas**
  Recursos como tuning de queries, indexação e particionamento exigem conhecimento mais profundo

* **Diferenças de comportamento em relação ao SQL Server**
  Pode impactar desenvolvedores acostumados com T-SQL (ex: funções, case sensitivity, sintaxe)

* **Gerenciamento operacional**
  Backup, replicação e tuning exigem configuração explícita (não tão “plug and play” quanto soluções gerenciadas)

* **Ferramentas corporativas mais limitadas**
  Em comparação com Oracle/SQL Server, o ecossistema enterprise pode ser menos integrado

* **Possível necessidade de tuning em escala**
  Para cargas muito altas, será necessário planejar:

  * índices adequados
  * particionamento
  * estratégias de leitura/escrita

---

## Notas Adicionais

* A estratégia inicial será utilizar migrations via Entity Framework Core
* Caso surjam necessidades específicas (ex: busca textual avançada ou cache), outros componentes poderão ser adicionados (ex: Redis, Elastic)
