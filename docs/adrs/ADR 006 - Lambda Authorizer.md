# ADR 006 - Lambda Authorizer no API Gateway

## Status

Aceito

## Contexto

O RFC 002 definiu que o cliente se autentica com o CPF numa Lambda que emite um JWT HS256, no mesmo
formato que a API já valida. Falta decidir como o **API Gateway** protege as rotas sensíveis antes de
encaminhar o tráfego ao ALB da aplicação no EKS.

Restrições:

* O token é assinado com chave simétrica (HS256), compartilhada com a API pelo secret `JWT_SIGNING_KEY`.
* A API autoriza por role com `[Authorize(Roles = ...)]` e tem endpoints anônimos que precisam
  continuar acessíveis: login de funcionário, primeiro acesso, acompanhamento de ordem de serviço,
  healthchecks e documentação.
* Existem dois ambientes, homologação e produção, cada um com seu hostname de API.

---

## Decisão

### 1. API Gateway HTTP com Lambda authorizer do tipo REQUEST

Cada ambiente tem um API Gateway HTTP (`wrench-api-gateway-<env>`) e uma Lambda authorizer
(`wrench-auth-authorizer-<env>`) com:

| Configuração | Valor | Motivo |
|---|---|---|
| Tipo | `REQUEST`, payload `2.0` | Formato do API Gateway HTTP |
| Resposta | simples (`isAuthorized` + `context`) | Dispensa montar política IAM |
| Identity source | `$request.header.Authorization` | Sem header, o gateway responde `401` sem invocar a function |
| Cache | 300 segundos por token | Limita custo e latência por requisição |
| Validação | assinatura HS256, issuer, audience e expiração | Mesmos parâmetros de `AuthenticationConfiguration` da API |

O `context` devolvido (`usuarioId`, `email`, `perfil`) vai para o access log do gateway, em JSON.

### 2. Roteamento por especificidade, com rotas públicas explícitas

| Rota | Integração | Authorizer |
|---|---|---|
| `POST /auth/cpf` | Lambda de autenticação (`AWS_PROXY`) | Não |
| `POST /api/v1/autenticacao`, `PUT /api/v1/usuario/primeiro-acesso`, `GET /api/v1/ordem-servico/{id}` | `HTTP_PROXY` para o ALB | Não |
| `GET /health`, `GET /health/ready`, `GET /docs-ui`, `GET /docs-ui/{proxy+}`, `GET /openapi/{proxy+}` | `HTTP_PROXY` para o ALB | Não |
| `GET /api/v1/ordem-servico/cliente`, `GET /api/v1/ordem-servico/monitoramento` | `HTTP_PROXY` para o ALB | **Sim** |
| `ANY /api/{proxy+}` | `HTTP_PROXY` para o ALB | **Sim** |

O padrão é **protegido**: toda rota da API cai em `ANY /api/{proxy+}`, e só as rotas anônimas da API
ganham rota pública explícita. As duas rotas literais de ordem de serviço existem porque o gateway
escolhe a rota mais específica, e `GET /api/v1/ordem-servico/{id}` capturaria `cliente` e
`monitoramento` como se fossem um id.

### 3. Defesa em profundidade: a API continua validando

O gateway repassa o header `Authorization` ao ALB. A API revalida o token e aplica as roles. O
authorizer decide **se** a requisição entra; a API decide **o que** ela pode fazer.

### 4. Controle de tráfego na borda

O stage aplica throttling (50 requisições por segundo sustentadas, rajada de 100) e grava access log
JSON com rota, status, latência, erro de integração e resultado do authorizer.

---

## Alternativas Consideradas

**JWT authorizer nativo do API Gateway HTTP.** Configuração declarativa, sem código e sem custo de
invocação. Descartado porque exige emissor OIDC com JWKS e algoritmo assimétrico; não valida HS256.
Adotá-lo significaria migrar emissão e validação para RS256, mudança registrada como questão em aberto
no RFC 002.

**Cognito User Pool authorizer.** Validação nativa e gestão de usuários pronta. Descartado pelo mesmo
motivo do RFC 002: autenticar só com CPF exige *custom auth flow* e a migração de todos os usuários
para o pool.

**Authorizer no mesmo handler da Lambda de autenticação.** Uma function a menos para implantar.
Descartado porque misturaria duas cargas com perfis opostos: o authorizer é chamado em quase toda
requisição e não precisa de banco; a autenticação é rara e precisa. Separados, o authorizer não
recebe connection string e escala sem abrir conexões com o RDS.

**Rota pública por padrão, protegendo rota a rota.** Menos rotas no gateway. Descartado porque
qualquer endpoint novo na API nasceria exposto na borda até alguém lembrar de protegê-lo.

**API Gateway REST em vez de HTTP.** Oferece WAF e usage plans. Descartado por custo por requisição
maior e configuração mais extensa, sem necessidade concreta desses recursos no projeto.

---

## Consequências

**Positivas**

* Requisições sem token válido são recusadas na borda e não consomem pods da aplicação.
* O formato de token e o modelo de autorização da API continuam os mesmos.
* Endpoint novo na API nasce protegido no gateway.
* O authorizer não tem acesso ao banco: comprometer essa function não expõe dados.
* Access log do gateway em JSON, com resultado do authorizer, complementa a observabilidade da API.

**Negativas**

* Um token recém-expirado continua passando pela borda até o fim do cache de 300 segundos. A API
  revalida a expiração, então o efeito é só consumir a aplicação nessa janela.
* O ALB continua acessível diretamente pelo hostname da API; a borda não é o único caminho. A
  revalidação na API cobre isso; restringir o ALB ao gateway exigiria VPC Link e ALB interno.
* Toda rota anônima nova na API precisa de uma rota pública correspondente no gateway, senão fica
  inacessível sem token. A lista precisa ser mantida junto com os `[AllowAnonymous]` da API.
* As rotas do gateway diferenciam maiúsculas de minúsculas: clientes precisam usar os caminhos em
  formato *slug* (`ordem-servico`), que é o formato gerado pela API.
* Invocações do authorizer somam latência na primeira requisição de cada token e custo por
  invocação, ambos limitados pelo cache.

---

## Referências

* [RFC 002 — Estratégia de Autenticação](../rfcs/RFC%20002%20-%20Estrategia%20de%20Autenticacao.md)
* [ADR 005 — Padrão de Comunicação entre Componentes](./ADR%20005%20-%20Padrao%20de%20Comunicacao.md)
* [Diagrama de sequência da autenticação](../diagramas/sequencia-autenticacao.md)
* Repositório `lambda-auth`: `terraform/api_gateway.tf`, `src/wrench.auto.lambda.auth/Funcoes/AuthorizerFunction.cs`
