# ADR 005 - Padrão de Comunicação entre Componentes

## Status

Aceito

## Contexto

O sistema é entregue por quatro componentes com ciclos de vida próprios:

* a **aplicação .NET** rodando no EKS, internamente dividida em quatro bounded contexts (autenticação, cadastro, estoque e ordem de serviço);
* a **Lambda de autenticação**, que valida o CPF, consulta o cliente e emite o JWT;
* o **API Gateway com Lambda authorizer**, ponto de entrada das rotas da aplicação;
* a **infraestrutura**, dividida entre plataforma Kubernetes e banco gerenciado.

Cada fronteira dessas exige uma decisão de comunicação, e elas não são a mesma decisão. Comunicação entre bounded contexts dentro do mesmo processo tem restrições diferentes de comunicação entre a borda e o cluster, que difere do acesso da Lambda ao banco, que por sua vez difere de como um repositório de infraestrutura publica valores para outro.

Sem um padrão explícito, o risco concreto é o acoplamento acidental: um contexto referenciando o repositório de outro, a aplicação chamando a Lambda em tempo de request, a Lambda dependendo do código da aplicação para compilar, ou um pipeline dependendo de outro pipeline para existir.

---

## Decisão

Adotamos **quatro padrões, um por tipo de fronteira**.

### 1. Entre bounded contexts, no mesmo processo: mensagens via mediator

Contextos não referenciam repositórios nem serviços uns dos outros. A comunicação é feita por mensagens despachadas pelo `IMediatorHandler` (MediatR), em duas modalidades:

* **Integrated query** (`ConsultaIntegrada`) — quando o contexto de origem precisa de uma resposta para prosseguir. É síncrona e o resultado influencia a decisão. Exemplo: ao abrir uma ordem de serviço, o contexto de ordem de serviço publica `VeiculoExisteEPertenceAoClienteQuery` e o contexto de cadastro responde.
* **Evento de domínio** (`PublicarEvento`) — quando o efeito é uma consequência, não uma pré-condição. É disparado após o commit e não altera o resultado da operação. Exemplo: `OrdemServicoAtualizadaEvent` dispara a notificação por e-mail.

A dependência de compilação é sempre com a **mensagem**, que vive em `wrench.auto.repair.core.Messages`, nunca com a implementação que a atende.

### 2. Entre a borda e a aplicação: API Gateway na frente do ALB, com JWT propagado

O **API Gateway HTTP API** fica na frente do ALB da aplicação, um por ambiente:

| Rota | Comportamento |
|---|---|
| `POST /auth/cpf` | integração `AWS_PROXY` com a Lambda de autenticação, sem authorizer |
| `ANY /api/{proxy+}` | **Lambda authorizer** e, se autorizado, `HTTP_PROXY` para o hostname da API no ambiente |
| login por e-mail e senha, `/health`, documentação | `HTTP_PROXY` sem authorizer |

O authorizer valida assinatura, emissor, audiência e expiração do token e devolve a decisão ao gateway. O `Authorization: Bearer` segue para a aplicação, que **revalida** o token e aplica as roles de cada endpoint — não confia apenas na borda. A escolha de authorizer por função, e não do JWT authorizer nativo, está no [ADR 006](./ADR%20006%20-%20Lambda%20Authorizer.md).

A aplicação **não chama a Lambda de autenticação**. O contrato entre as duas é o formato do token: mesmo `Issuer`, mesmo `Audience`, mesma chave de assinatura e as claims `NameIdentifier`, `Name` e `Role`. Isso mantém as duas independentes em runtime: a Lambda pode ser reimplantada sem afetar a aplicação, e vice-versa.

### 3. Entre a Lambda e o banco: contrato de colunas, sem código compartilhado

A Lambda de autenticação não referencia nem copia projetos da aplicação. Ela tem um `DbContext` próprio, somente leitura e sem migrations, que mapeia apenas as colunas de que precisa:

| Tabela | Colunas |
|---|---|
| `Clientes` | `Id`, `Documento`, `Email` |
| `Usuarios` | `Id`, `Email`, `PerfilId`, `Ativo` |
| `Perfis` | `Id`, `Nome` |

A aplicação continua dona do schema. O contrato é garantido em dois lugares: um **teste de contrato** no pipeline da aplicação verifica essas colunas depois das migrations, e o role de banco da Lambda recebe **`GRANT SELECT` apenas nessas colunas**.

### 4. Entre repositórios de infraestrutura: remote state, não pipeline

Valores que atravessam repositórios (endpoint do banco, nome do cluster, ARN do certificado, URLs do ECR) são publicados como **outputs de state no HCP Terraform** e lidos por `terraform_remote_state`.

Nenhum pipeline dispara outro pipeline, e nenhum pipeline depende de artefato produzido por outro na mesma execução.

---

## Alternativas Consideradas

**Chamadas HTTP diretas entre bounded contexts.** Daria independência de deploy futura, mas os contextos hoje rodam no mesmo processo e compartilham transação. Introduzir HTTP aqui traria latência, falha parcial e complexidade de retry sem resolver problema existente. A separação por mensagens já preserva a fronteira lógica e permite extrair um contexto para fora do processo depois, trocando o handler sem tocar em quem publica.

**Barramento de mensageria (SQS/SNS/RabbitMQ) para os eventos de domínio.** Traria entrega garantida e desacoplamento temporal, inclusive para as notificações por e-mail. Foi descartado por custo e complexidade operacional desproporcionais ao volume do projeto: exigiria fila, dead-letter queue, idempotência e monitoramento próprio. Fica registrado como evolução natural quando o volume de notificações justificar.

**API Gateway em paralelo ao ALB, protegendo só a autenticação.** Manteria o ALB como entrada das rotas de negócio e deixaria o gateway só na frente da Lambda. Descartado porque não protege as rotas sensíveis na borda, que é o propósito do gateway.

**A aplicação chamando a Lambda para validar o token.** Centralizaria a lógica de validação, mas colocaria a Lambda no caminho crítico de toda requisição autenticada — somando latência, custo por invocação e um ponto de falha novo. Validação de JWT é verificação local de assinatura.

**A Lambda referenciando ou copiando os projetos da aplicação.** Reaproveitaria `CpfCnpj`, repositórios e gerador de token. Por referência, o repositório da Lambda deixa de compilar sozinho; por cópia, as duas versões divergem sem aviso. O contrato de colunas entrega o mesmo resultado com um teste que falha no lado de quem muda.

**A Lambda chamando um endpoint da aplicação para consultar o cliente.** Evitaria o acesso direto ao banco, mas a Lambda precisaria de credencial para chamar a API que ela mesma protege, e uma indisponibilidade da aplicação impediria qualquer login.

**Cross-repository workflow dispatch entre os pipelines.** Reproduziria uma orquestração central, mas criaria acoplamento temporal entre repositórios: o deploy da aplicação passaria a depender de o pipeline de infraestrutura ter rodado naquele momento. O remote state entrega o mesmo valor sem essa dependência.

---

## Consequências

**Positivas**

* Os bounded contexts continuam extraíveis: trocar um handler in-process por um adaptador HTTP não muda quem publica a mensagem.
* Rotas de negócio só são aceitas na borda com token válido; requisições negadas não consomem a aplicação.
* A Lambda e a aplicação evoluem em ritmos independentes; os contratos de token e de colunas são verificáveis em teste.
* Cada repositório é implantável isoladamente. Um `apply` de infraestrutura falhando não impede um hotfix na aplicação.
* A revalidação do token na aplicação mantém a segurança mesmo se alguém alcançar o ALB sem passar pelo gateway.

**Negativas**

* O ALB continua público. A proteção de quem o acessa diretamente depende da revalidação na aplicação.
* A chave de assinatura do JWT é compartilhada entre dois repositórios. Rotacioná-la exige coordenação, e um descompasso se manifesta como `401` ou `403` genérico.
* A Lambda depende de nomes de tabela e coluna do schema da aplicação. Renomear uma dessas colunas exige mudança coordenada, sinalizada pelo teste de contrato.
* Os `GRANT` por coluna só podem ser aplicados depois de as tabelas existirem, o que impõe ordem no primeiro provisionamento.
* Eventos de domínio in-process não têm garantia de entrega; uma falha de envio de e-mail não desfaz a operação e aparece apenas na telemetria.
* Ler remote state exige compartilhamento de state entre workspaces no HCP; o erro só aparece no `plan`.
* A validação do token acontece duas vezes por requisição. O custo é desprezível, mas é duplicação real de responsabilidade.

---

## Referências

* [Diagrama de componentes](../infraestrutura/arquitetura.md)
* [Diagrama de sequência — autenticação](../diagramas/sequencia-autenticacao.md)
* [Diagrama de sequência — abertura de ordem de serviço](../diagramas/sequencia-abertura-ordem-servico.md)
* [ADR 006 — Lambda Authorizer](./ADR%20006%20-%20Lambda%20Authorizer.md)
* [Modelo relacional](../database/modelo-relacional.md)
