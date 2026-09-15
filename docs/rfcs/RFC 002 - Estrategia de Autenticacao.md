# RFC 002 — Estratégia de Autenticação

| | |
|---|---|
| **Status** | Aprovado |
| **Autor** | Bruno Barreto |
| **Revisores** | Thiago Ribeiro, João Paulo Seixas |
| **Data** | Setembro de 2026 |
| **Fase** | Tech Challenge — Fase 3 |
| **Substitui** | — |
| **Relacionado** | ADR 005 (comunicação), ADR 006 (Lambda authorizer), RFC 001 (nuvem) |

## Resumo

Este RFC define como o cliente da oficina se autentica com o CPF e como as rotas sensíveis da API são
protegidas. A proposta é uma **Lambda de autenticação** que valida o documento, consulta existência e
status do cliente e emite um JWT, e um **API Gateway com Lambda authorizer** que valida esse token
antes de encaminhar o tráfego ao ALB da aplicação. A API mantém a validação do token e das roles.

## Contexto e motivação

Até a Fase 2, a autenticação acontecia dentro do monólito: funcionários enviam e-mail e senha, e a
API emite o JWT. O ALB só roteava, sem nenhuma verificação na borda. A Fase 3 exige:

- um API Gateway como ponto de controle e roteamento;
- rotas sensíveis protegidas por autenticação via CPF;
- uma function serverless que valide o CPF, consulte existência e status do cliente e gere o JWT.

Três restrições moldam a solução:

1. **O token precisa ser aceito pela API sem mudança no modelo de autorização.** Os controllers usam
   `[Authorize(Roles = ...)]` sobre JWT HS256 com issuer e audience `Wrench Auto Repair`. Trocar o
   emissor não pode obrigar a reescrever a autorização.
2. **A function vive em repositório separado.** Ela não pode referenciar nem copiar os projetos da
   API, senão o repositório deixa de compilar sozinho ou passa a manter código duplicado.
3. **Custo acadêmico.** Nada que cobre por usuário ativo ou exija infraestrutura dedicada.

## Proposta

### Emissão do token

`POST /auth/cpf` no API Gateway invoca a Lambda `wrench-auth-cpf-<env>`, que:

1. valida os dígitos verificadores com a mesma biblioteca do value object `CpfCnpj` da API
   (`DocsBRValidator`) e normaliza para somente dígitos;
2. consulta cliente, usuário vinculado (pelo e-mail) e perfil;
3. responde `404` se não houver cliente ou usuário, `403` se o usuário estiver inativo ou não tiver o
   perfil `Cliente`;
4. emite o JWT com as claims que a API já usa: `NameIdentifier`, `Name` e `Role`.

A restrição ao perfil `Cliente` fecha uma escalada de privilégio: um funcionário que também esteja
cadastrado como cliente com o mesmo e-mail receberia token de funcionário informando apenas o CPF,
sem senha.

### Proteção das rotas

O API Gateway `wrench-api-gateway-<env>` encaminha `/api/...` ao ALB por integração `HTTP_PROXY`. As
rotas protegidas usam o Lambda authorizer `wrench-auth-authorizer-<env>`, que valida assinatura,
issuer, audience e expiração com os mesmos parâmetros da API. As rotas públicas espelham os
endpoints `[AllowAnonymous]` da API. O detalhamento está no ADR 006.

### Como a Lambda consulta o cliente

| Opção | Como funciona | Avaliação |
|---|---|---|
| **Leitura direta no RDS com EF Core e GRANT por coluna** | `DbContext` próprio, somente leitura e sem migrations, mapeando só `Clientes(Id, Documento, Email)`, `Usuarios(Id, Email, PerfilId, Ativo)` e `Perfis(Id, Nome)`. Role `wrench_lambda_auth` com `SELECT` apenas nessas colunas | **Escolhida.** Sem dependência de disponibilidade da API, sem código compartilhado, menor privilégio no nível da coluna e mesma tecnologia de acesso a dados que o time já usa |
| Endpoint da API chamado pela Lambda | A Lambda consulta o contexto de cadastro por HTTP | Descartada. Para chamar a API a Lambda precisaria de uma credencial, que é justamente o que ela emite; e a indisponibilidade da API impediria qualquer login |
| Read model alimentado por eventos | A API publica cadastro e ativação de usuário num barramento; a Lambda mantém uma tabela própria (DynamoDB) | Descartada nesta fase. É o desacoplamento mais forte, mas os eventos hoje são in-process (MediatR) e a mudança exigiria broker, publisher, consumidor e reprocessamento |
| Referenciar ou copiar os projetos da API | A Lambda reaproveita repositórios e handlers da API | Descartada. Quebra a independência do repositório ou duplica código de domínio |
| View no banco como contrato | A API publica uma view com as colunas necessárias | Descartada. Adiciona um objeto de banco a manter nas migrations; o GRANT por coluna e o teste de contrato entregam o mesmo isolamento |

O acoplamento pelo schema é explícito e verificado dos dois lados: o teste de integração da Lambda
conecta com um role que tem exatamente os grants de produção, e o `app-k8s` tem um teste que falha
se uma migration remover ou renomear uma das colunas do contrato.

## Alternativas avaliadas para a borda

### Amazon Cognito

Pool de usuários gerenciado, com JWT assinado em RS256 e JWKS público. Resolveria rotação de chave e
revogação, e o API Gateway valida tokens do Cognito nativamente. Descartado porque:

- autenticar só com CPF, sem senha, exige *custom authentication flow* com três triggers Lambda
  (define, create e verify challenge), mais complexo que a Lambda única;
- a API passaria a validar pelo JWKS do Cognito e todos os usuários, inclusive funcionários,
  precisariam migrar para o pool;
- cobra por usuário ativo mensal acima da camada gratuita.

### JWT authorizer nativo do API Gateway HTTP

Validação de token sem código, configurada por issuer e audience. **Não atende**: o authorizer nativo
exige um emissor OIDC com JWKS publicado e só aceita algoritmos assimétricos. O token do sistema é
HS256 com chave simétrica, e publicar JWKS exigiria migrar emissão e validação para RS256.

### Autenticação apenas na API

Manter o API Gateway só como roteador e deixar a API validar o token. É o comportamento da Fase 2.
Não cumpre o requisito de proteção na borda: requisições sem token consomem a aplicação, e o gateway
não oferece nenhum controle.

### Lambda authorizer — escolhido

Uma function curta que valida o JWT com a mesma chave e os mesmos parâmetros da API. Preserva o
formato de token existente, não exige migração de usuários e mantém a validação na borda. O custo
por invocação é controlado pelo cache de 300 segundos por token.

## Gestão da chave de assinatura

A chave HS256 é compartilhada por três consumidores: Lambda de autenticação, authorizer e API. Ela
vive no secret `JWT_SIGNING_KEY` dos repositórios `lambda-auth` e `app-k8s`, é injetada como
variável de ambiente na Lambda e como Secret Kubernetes na API, e nunca é versionada. As três
functions recusam subir sem a chave ou com menos de 32 caracteres.

**Rotação.** Trocar a chave invalida todos os tokens emitidos, que vivem no máximo 30 minutos. O
procedimento é atualizar o secret nos dois repositórios e reexecutar os dois pipelines em sequência;
durante a janela entre os deploys, tokens novos podem ser recusados pela API. Eliminar essa janela
exige aceitar duas chaves ao mesmo tempo na validação, o que fica como evolução junto com a migração
para Secrets Manager.

## Riscos

| Risco | Mitigação |
|---|---|
| Acesso direto ao ALB contorna o gateway | A API revalida o token e as roles em toda requisição |
| Token expirado passa pela borda dentro do TTL do cache | Mesma revalidação na API; TTL limitado a 300 segundos |
| Migration altera coluna do contrato | Teste de contrato no `app-k8s` e teste de integração com grants reais na Lambda |
| Autenticação só com CPF é um fator fraco | Restrita ao perfil `Cliente`, cujas operações se limitam à própria ordem de serviço; funcionários continuam exigindo senha |
| Chave em variável de ambiente da Lambda | Criptografada em repouso pela AWS; migração para Secrets Manager registrada como evolução |

## Questões em aberto

1. Migrar emissão e validação para RS256 com JWKS, o que permitiria usar o JWT authorizer nativo e
   eliminar a Lambda authorizer.
2. Adotar Secrets Manager com rotação automática da chave.
3. Adicionar um segundo fator para o cliente (código por e-mail via SES) se o escopo das operações do
   perfil `Cliente` crescer.

## Referências

* [ADR 006 — Lambda Authorizer no API Gateway](../adrs/ADR%20006%20-%20Lambda%20Authorizer.md)
* [ADR 005 — Padrão de Comunicação entre Componentes](../adrs/ADR%20005%20-%20Padrao%20de%20Comunicacao.md)
* [Diagrama de sequência da autenticação](../diagramas/sequencia-autenticacao.md)
* Repositório `lambda-auth`: README e `docs/openapi.yaml`
