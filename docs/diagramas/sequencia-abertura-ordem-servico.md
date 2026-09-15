# Diagrama de sequência — Abertura de ordem de serviço

Fluxo de `POST /api/v1/ordem-servico`, da borda autenticada até a notificação por e-mail.

O endpoint existe em duas variantes:

- **`POST /api/v1/ordem-servico`** — cliente e veículo já cadastrados; recebe os identificadores.
- **`POST /api/v1/ordem-servico/servico`** — atendimento de balcão: cadastra cliente e veículo na
  mesma requisição e então abre a ordem.

O diagrama principal cobre a primeira; a segunda está logo abaixo. O token usado aqui é obtido no
[fluxo de autenticação](./sequencia-autenticacao.md).

## Fluxo principal

```mermaid
sequenceDiagram
    autonumber
    actor Atendente
    participant GW as API Gateway
    participant AZ as Lambda authorizer
    participant ALB as ALB / Gateway API
    participant Ctrl as OrdemServicoController
    participant Med as IMediatorHandler<br/>(MediatR)
    participant Hnd as OrdemServicoCommandHandler
    participant Cad as Contexto Cadastro<br/>(consulta integrada)
    participant Dom as OrdemServico<br/>(entidade de domínio)
    participant Repo as IOrdemServicoRepository
    participant DB as RDS PostgreSQL
    participant Evt as OrdemServicoAtualizadaEventHandler
    participant SES as Amazon SES

    Atendente->>GW: POST /api/v1/ordem-servico<br/>Authorization: Bearer JWT
    alt cabeçalho Authorization ausente
        GW-->>Atendente: 401 Unauthorized
    end
    GW->>AZ: evento REQUEST com o cabeçalho
    AZ->>AZ: valida assinatura, issuer,<br/>audience e expiração
    AZ-->>GW: isAuthorized
    alt token inválido ou expirado
        GW-->>Atendente: 403 Forbidden
    end
    GW->>ALB: HTTP_PROXY https://api.bgt3.com.br/api/v1/ordem-servico
    ALB->>Ctrl: Post(CriarOrdemServicoRequest)
    Ctrl->>Ctrl: JwtBearer revalida o token<br/>e exige role Admin ou Funcionario
    alt role sem permissão
        Ctrl-->>Atendente: 403 Forbidden
    end

    Ctrl->>Med: EnviarComando<CriarOrdemServicoCommand, Guid>
    Med->>Hnd: Handle(command)

    Hnd->>Hnd: command.EhValido()
    alt comando inválido
        Hnd-->>Ctrl: Result.ValidationError
        Ctrl-->>Atendente: 400 Bad Request
    end

    Hnd->>Med: ConsultaIntegrada<br/>(VeiculoExisteEPertenceAoClienteQuery)
    Med->>Cad: resolve consulta
    Cad->>DB: SELECT veículo do cliente
    DB-->>Cad: registro
    Cad-->>Hnd: Sucesso / Falha

    alt veículo não pertence ao cliente
        Hnd-->>Ctrl: Result.NotFound
        Ctrl-->>Atendente: 404 Not Found
    end

    Hnd->>Dom: new OrdemServico(...)<br/>status = Recebida
    Dom->>Dom: valida invariantes
    Hnd->>Repo: Adicionar(ordemServico)
    Hnd->>Repo: UnitOfWork.CommitAsync()
    Repo->>DB: INSERT ordem de serviço
    DB-->>Repo: linhas afetadas

    alt commit não persistiu
        Hnd-->>Ctrl: Result.Unexpected
        Ctrl-->>Atendente: 500 Internal Server Error
    end

    Hnd->>Med: PublicarEvento<br/>(OrdemServicoAtualizadaEvent)
    Med-->>Evt: Handle(evento)
    Hnd-->>Ctrl: Result.Created(id)
    Ctrl-->>Atendente: 201 Created + id

    Note over Evt,SES: Notificação segue fora do caminho da resposta
    Evt->>Cad: ObterClientePorIdQuery
    Cad-->>Evt: ClienteViewModel (nome, e-mail)
    Evt->>Repo: ObterPorIdAsync(ordemServicoId)
    Repo-->>Evt: ordem de serviço
    Evt->>Evt: renderiza template<br/>OrdemServicoAtualizacao
    Evt->>SES: EnviarAsync(mensagem)
    SES-->>Atendente: e-mail de acompanhamento ao cliente
```

## Pontos de atenção do fluxo

**A autorização acontece em duas camadas.** O authorizer do API Gateway recusa na borda qualquer
token inválido, antes de a requisição chegar ao cluster. A aplicação revalida o mesmo token e aplica
a role exigida pelo endpoint; é essa segunda camada que decide entre `Admin`, `Funcionario` e
`Cliente`, e que protege a API de quem alcança o ALB sem passar pelo gateway.

**A validação acontece em três níveis.** O `EhValido()` do comando roda as regras de
FluentValidation sobre o formato da entrada; a consulta integrada confirma que o veículo existe e
pertence àquele cliente; e o construtor da entidade `OrdemServico` valida as invariantes de
domínio. Cada nível devolve um `TipoErroEnum` diferente, que o `ResultExtensions` traduz para o
status HTTP correspondente.

**A checagem de veículo cruza bounded contexts sem acoplar código.** O contexto de ordem de
serviço não referencia o repositório de cadastro: ele publica uma *integrated query* no mediator
(`VeiculoExisteEPertenceAoClienteQuery`) e quem responde é o contexto de cadastro. A dependência é
com a mensagem, não com a implementação.

**O e-mail não bloqueia a resposta.** A notificação é disparada por evento
(`OrdemServicoAtualizadaEvent`) e o handler engole exceções do envio: uma indisponibilidade do SES
não pode transformar uma ordem de serviço já persistida em erro para o atendente. O efeito
colateral é que uma falha de envio não aparece para o atendente: ela é visível apenas na telemetria,
no painel de erros de integração do New Relic, que lê as chamadas HTTP de saída com erro.

**O commit é explícito.** O repositório não salva sozinho; `UnitOfWork.CommitAsync()` delimita a
transação, e o handler decide o resultado a partir do número de linhas afetadas.

## Variante de balcão

`POST /api/v1/ordem-servico/servico` resolve cliente e veículo antes de abrir a ordem, abortando no
primeiro erro:

A requisição passa pela mesma borda do fluxo principal (API Gateway, authorizer e ALB), omitida
abaixo.

```mermaid
sequenceDiagram
    autonumber
    actor Atendente
    participant Ctrl as OrdemServicoController
    participant Med as IMediatorHandler
    participant Cad as Contexto Cadastro
    participant OS as Contexto Ordem de Serviço

    Atendente->>Ctrl: POST /ordem-servico/servico<br/>{cliente, veículo, descrição}

    Ctrl->>Med: CadastrarClienteCommand
    Med->>Cad: cria cliente
    Cad-->>Ctrl: Result<Guid> clienteId
    alt falha no cliente
        Ctrl-->>Atendente: erro do cadastro de cliente
    end

    Ctrl->>Med: CadastrarVeiculoCommand<br/>(ClienteId = clienteId)
    Med->>Cad: cria veículo
    Cad-->>Ctrl: Result<Guid> veiculoId
    alt falha no veículo
        Ctrl-->>Atendente: erro do cadastro de veículo
    end

    Ctrl->>Med: CriarOrdemServicoCommand<br/>(clienteId, veiculoId, descrição)
    Med->>OS: fluxo principal acima
    OS-->>Ctrl: Result<Guid> ordemServicoId
    Ctrl-->>Atendente: 201 Created + id
```

Os três comandos rodam em transações separadas, uma por contexto. Um erro no terceiro passo deixa
cliente e veículo já cadastrados — comportamento aceitável no domínio, já que ambos são cadastros
legítimos e reaproveitáveis na próxima tentativa.

## Ciclo de vida da ordem

A abertura cria a ordem no status `Recebida`. As transições seguintes têm endpoints próprios e
cada mudança de status republica o `OrdemServicoAtualizadaEvent`, gerando nova notificação:

```mermaid
stateDiagram-v2
    [*] --> Recebida: POST /ordem-servico
    Recebida --> EmDiagnostico: solicitar diagnóstico
    EmDiagnostico --> AguardandoAprovacao: registrar diagnóstico<br/>e orçamento
    AguardandoAprovacao --> EmExecucao: cliente aprova
    AguardandoAprovacao --> [*]: cliente recusa
    EmExecucao --> Finalizada: finalizar
    Finalizada --> Entregue: entregar veículo
    Entregue --> [*]
```

## Documentos relacionados

- [Diagrama de componentes](../infraestrutura/arquitetura.md)
- [Pipeline HTTP e healthchecks](../infraestrutura/pipeline-http.md)
- [Linguagem ubíqua](../linguagem-ubiqua/linguage-ubiqua.md)
- [Diagrama de sequência — autenticação](./sequencia-autenticacao.md)
- [ADR 005 — Padrão de comunicação](../adrs/ADR%20005%20-%20Padrao%20de%20Comunicacao.md)
- [Dashboards e alertas](../observability/dashboards-nrql.md)
