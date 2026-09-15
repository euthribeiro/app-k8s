# Diagrama de sequência — Autenticação por CPF e consumo de rota protegida

Dois fluxos encadeados: o cliente troca o CPF por um JWT na Lambda de autenticação e depois usa esse
token nas rotas protegidas, que passam pelo Lambda authorizer antes de chegar à API no EKS.

Componentes, por repositório:

| Componente | Repositório | Recurso |
|---|---|---|
| API Gateway HTTP | `lambda-auth` | `wrench-api-gateway-<env>` |
| Lambda de autenticação | `lambda-auth` | `wrench-auth-cpf-<env>` |
| Lambda authorizer | `lambda-auth` | `wrench-auth-authorizer-<env>` |
| RDS PostgreSQL | `infra-db` | role `wrench_lambda_auth`, `SELECT` por coluna |
| ALB e API .NET | `app-k8s` / `infra-k8s` | `api.bgt3.com.br`, `hml-api.bgt3.com.br` |

## 1. Autenticação por CPF

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant GW as API Gateway
    participant Auth as Lambda de autenticação<br/>AutenticacaoFunction
    participant Doc as DocumentoCliente<br/>(DocsBRValidator)
    participant EF as AutenticacaoDbContext<br/>(EF Core, somente leitura)
    participant DB as RDS PostgreSQL
    participant Tok as GeradorToken
    participant Logs as CloudWatch Logs

    Cliente->>GW: POST /auth/cpf<br/>{ "documento": "529.982.247-25" }
    GW->>Auth: invoca (AWS_PROXY, payload 2.0)

    alt corpo ausente ou JSON inválido
        Auth-->>GW: 400 Corpo da requisição inválido
        GW-->>Cliente: 400
    end

    Auth->>Doc: TentarNormalizar(documento)
    alt dígitos verificadores inválidos
        Doc-->>Auth: false
        Auth-->>GW: 400 Documento inválido
        GW-->>Cliente: 400
    end
    Doc-->>Auth: "52998224725"

    Auth->>EF: ObterPorDocumentoAsync("52998224725")
    EF->>DB: SELECT u."Id", u."Email", p."Nome", u."Ativo"<br/>FROM "Clientes" c<br/>JOIN "Usuarios" u ON c."Email" = u."Email"<br/>JOIN "Perfis" p ON u."PerfilId" = p."Id"<br/>WHERE c."Documento" = @documento
    DB-->>EF: linha ou vazio
    EF-->>Auth: CredencialCliente ou null

    alt cliente inexistente ou sem usuário vinculado
        Auth-->>GW: 404 Cliente não encontrado
        GW-->>Cliente: 404
    else perfil diferente de Cliente
        Auth-->>GW: 403 apenas para clientes
        GW-->>Cliente: 403
    else usuário inativo
        Auth-->>GW: 403 Cliente inativo
        GW-->>Cliente: 403
    end

    Auth->>Tok: Gerar(credencial)
    Tok-->>Auth: JWT HS256<br/>nameid · name · role=Cliente<br/>iss/aud Wrench Auto Repair · exp 30 min
    Auth->>Logs: {"level":"Information","message":"Status=Autenticado CorrelationId=..."}
    Auth-->>GW: 200 { token, username, role }
    GW-->>Cliente: 200 + X-Correlation-ID
```

O documento não é registrado em log. Uma exceção inesperada (banco indisponível, por exemplo) gera
`500` com mensagem genérica, e o detalhe fica apenas no log da function.

## 2. Consumo de rota protegida

Exemplo: o cliente aprova o orçamento da sua ordem de serviço, endpoint que exige a role `Cliente`.

```mermaid
sequenceDiagram
    autonumber
    actor Cliente
    participant GW as API Gateway
    participant Authz as Lambda authorizer<br/>AuthorizerFunction
    participant ALB as ALB / Gateway API
    participant API as API .NET<br/>JwtBearer + [Authorize(Roles)]
    participant Ctrl as OrcamentoController
    participant DB as RDS PostgreSQL

    Cliente->>GW: PUT /api/v1/orcamento/{id}/aprovar<br/>Authorization: Bearer JWT

    alt header Authorization ausente
        GW-->>Cliente: 401 Unauthorized<br/>(authorizer não é invocado)
    end

    alt decisão em cache para este token (TTL 300 s)
        GW->>GW: reaproveita a decisão
    else sem cache
        GW->>Authz: REQUEST (payload 2.0)<br/>identity source: Authorization
        Authz->>Authz: valida assinatura HS256, issuer,<br/>audience e expiração
        alt token inválido, expirado ou sem "Bearer"
            Authz-->>GW: { isAuthorized: false }
            GW-->>Cliente: 403 Forbidden
        end
        Authz-->>GW: { isAuthorized: true,<br/>context: usuarioId, email, perfil }
    end

    GW->>ALB: HTTP_PROXY https://api.bgt3.com.br/api/v1/orcamento/{id}/aprovar<br/>repassa Authorization e X-Correlation-ID
    ALB->>API: requisição
    API->>API: revalida o JWT com a mesma chave
    alt role diferente de Cliente
        API-->>Cliente: 403 Forbidden
    end
    API->>Ctrl: AprovarOrcamento(id)
    Ctrl->>DB: atualiza ordem de serviço
    DB-->>Ctrl: ok
    Ctrl-->>API: 200
    API-->>ALB: 200
    ALB-->>GW: 200
    GW-->>Cliente: 200
```

## Pontos de atenção

- **Duas validações do mesmo token.** O authorizer recusa na borda, sem consumir a aplicação; a API
  revalida e aplica as roles. A segunda validação cobre o acesso direto ao ALB e a janela de cache
  do authorizer, em que um token recém-expirado ainda passa pela borda.
- **Contrato do token.** Chave, issuer e audience são idênticos na Lambda, no authorizer e na API; a
  chave vem do secret `JWT_SIGNING_KEY` nos repositórios `lambda-auth` e `app-k8s`.
- **Rotas públicas.** `POST /auth/cpf`, `POST /api/v1/autenticacao`, `PUT /api/v1/usuario/primeiro-acesso`,
  `GET /api/v1/ordem-servico/{id}`, healthchecks e documentação não passam pelo authorizer, espelhando
  os endpoints `[AllowAnonymous]` da API.

## Referências

- [RFC 002 — Estratégia de autenticação](../rfcs/RFC%20002%20-%20Estrategia%20de%20Autenticacao.md)
- [ADR 006 — Lambda Authorizer no API Gateway](../adrs/ADR%20006%20-%20Lambda%20Authorizer.md)
- [Diagrama de sequência — abertura de ordem de serviço](./sequencia-abertura-ordem-servico.md)
- Repositório `lambda-auth`: README e `docs/openapi.yaml`
