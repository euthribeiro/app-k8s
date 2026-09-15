# Migrations do Entity Framework Core

A aplicação usa **quatro `DbContext` independentes**, um por bounded context. Cada um tem seu
próprio conjunto de migrations, na pasta `Migrations/` do respectivo projeto de infraestrutura.
Não existe migration "global": toda operação de EF precisa dizer explicitamente qual contexto
está sendo alvo, via `--context`.

| Contexto | Projeto (`--project`) | Domínio coberto |
|---|---|---|
| `AutenticacaoContext` | `src/wrench.auto.repair.autenticacao.infra` | Usuários, perfis e credenciais |
| `CadastroContext` | `src/wrench.auto.repair.cadastro.infra` | Clientes, veículos e endereços |
| `PecaDbContext` | `src/wrench.auto.repair.estoque.infra` | Peças e estoque |
| `OrdemServicoDbContext` | `src/wrench.auto.repair.ordem.servico.infra` | Ordens de serviço e diagnósticos |

O projeto de inicialização (`--startup-project`) é sempre `src/wrench.web.api`, porque é ele que
carrega a configuração de connection string e registra os contextos no container de injeção de
dependência.

## Criar uma migration

Rodando a partir da raiz da solução (`wrench.auto.repair/`):

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project ./src/wrench.auto.repair.ordem.servico.infra \
  --startup-project ./src/wrench.web.api \
  --context OrdemServicoDbContext
```

Troque `--project` e `--context` conforme a tabela acima.

## Aplicar migrations manualmente

```bash
dotnet ef database update \
  --project ./src/wrench.auto.repair.ordem.servico.infra \
  --startup-project ./src/wrench.web.api \
  --context OrdemServicoDbContext
```

## Aplicação automática na subida

Em execução normal a aplicação **não depende** do comando acima: `Program.cs` chama
`ApplyMigrationsAsync()` no start, aplicando as migrations pendentes de todos os contextos antes
de aceitar tráfego. O endpoint `/health/ready` só responde `200` depois que os quatro contextos
estão migrados — é isso que o *readiness probe* do Kubernetes consulta, de forma que um pod com
migration pendente não entra no balanceador.

O `dotnet ef database update` fica reservado para operação manual: inspecionar o SQL gerado,
reparar um ambiente ou aplicar a migration fora do ciclo de deploy.

## Convenção de persistência

Entidades e value objects expõem um construtor sem parâmetros `private`/`protected` usado
exclusivamente pelo EF Core para materializar o objeto vindo do banco — ele não faz parte da API
pública do domínio e não executa as validações de invariante, que ficam no construtor real.
Propriedades de coleção que representam relacionamento são propriedades de navegação mapeadas
pelo EF; ambas estão marcadas com documentação XML no código.

O `OrdemServicoDbContext` registra ainda a função de banco `datediff_milliseconds`
(`PostgresDbFunctions.DateDiffMilliseconds`), usada para calcular tempo de execução de ordem de
serviço diretamente em SQL, sem trazer as linhas para a memória.
