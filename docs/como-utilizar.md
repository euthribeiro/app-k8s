# ROTEIRO DE USO WRENCH AUTO REPAIR API - V1

Antes de utilizar a API, é fundamental compreender sua arquitetura, responsabilidades de cada recurso e fluxo operacional.

## Sumário

1. Tecnologias
2. Versionamento
3. Segurança e Autenticação
4. Gerenciamento de Respostas
5. Serviços da API
6. Passo-a-passo de uso

---

## Objetivo

Este documento tem como objetivo descrever de forma clara e objetiva como consumir a API da **Wrench Auto Repair**, incluindo fluxos completos de utilização e regras implícitas do domínio.

---

## 1. Tecnologias

* C# / ASP.NET Core Web API
* Arquitetura **CQRS com Mediator**
* Docker (ambiente containerizado)
* PostgreSQL (persistência relacional)

---

## 2. Versionamento

A API utiliza versionamento via URL:

**Padrão:**

```
/api/v{version}/recurso
```

**Exemplo:**

```
POST /api/v1/autenticacao
```

O versionamento é obrigatório em todas as requisições.

---

## 3. Segurança e Autenticação

A API utiliza autenticação baseada em **JWT (Bearer Token)** 

Após autenticação, o token deve ser enviado no header:

```
Authorization: Bearer {token}
```

### Observações importantes:

* O token possui tempo de expiração configurável (default: 30 minutos)
* Todos os endpoints (exceto autenticação) exigem autenticação
* Controle de acesso é baseado em **perfil de usuário**

---

## 4. Gerenciamento de Respostas

A API segue convenções HTTP:

| Código | Descrição                                                           |
| ------ | ------------------------------------------------------------------- |
| 200    | Requisição processada com sucesso                                   |
| 201    | Recurso criado                                                      |
| 204    | Sucesso sem conteúdo                                                |
| 400    | Erro de validação                                                   |
| 401    | Não autenticado / token inválido                                    |
| 403    | Sem permissão                                                       |
| 404    | Recurso não encontrado                                              |
| 409    | Conflito de estado                                                  |
| 422    | Erro semântico                                                      |
| 500    | Erro interno                                                        |

---

## 5. Serviços da API

### 5.1 Autenticação

#### POST /api/v{version}/autenticacao

Responsável por gerar o token JWT.

**Request:**

```json
{
  "email": "usuario@wrench.com.br",
  "senha": "senha"
}
```

---

### 5.2 Cliente

#### GET /cliente

Lista clientes (paginado, ordenável)

#### GET /cliente/{id}

Busca por ID (UUID)

#### GET /cliente/{documento}

Busca por CPF/CNPJ

#### POST /cliente

Cadastra cliente

#### PUT /cliente

Atualiza cliente

**Payload de atualização:**

```json
{
  "clienteId": "uuid",
  "nome": "string",
  "telefone": "string",
  "email": "string",
  "endereco": { }
}
```

---

### 5.3 Usuário

#### GET /usuario

Lista usuários

#### GET /usuario/{id}

Busca usuário

#### POST /usuario

Cria usuário

#### DELETE /usuario/{id}

Inativa usuário

#### PUT /usuario/{id}/ativar

Reativa usuário

#### PUT /usuario/primeiro-acesso

Define senha inicial

#### PUT /usuario/{id}/resetar-acesso

Reseta credenciais

---

### 5.4 Perfil

#### GET /perfil

Lista perfis disponíveis

---

### 5.5 Peças / Estoque

#### GET /peca

Lista peças (paginado)

#### GET /peca/{id}

Busca peça

#### POST /peca

Cria peça

#### PUT /peca

Atualiza peça

#### PUT /peca/{id}/repor

Entrada de estoque

#### PUT /peca/{id}/baixar

Saída de estoque

#### PUT /peca/{id}/ativar

Ativa item

#### PUT /peca/{id}/desativar

Inativa item

---

### 5.6 Veículo

#### GET /veiculo

Lista veículos

#### GET /veiculo/{id}

Busca por ID

#### GET /veiculo/{placa}

Busca por placa

#### POST /veiculo

Cadastra veículo

#### PUT /veiculo

Atualiza veículo

---

### 5.7 Ordem de Serviço

#### POST /ordem-servico

Cria ordem de serviço

#### GET /ordem-servico

Lista ordens

#### GET /ordem-servico/{id}

Consulta ordem

#### GET /ordem-servico/cliente

Lista por cliente/veículo

#### GET /ordem-servico/monitoramento

Visão operacional

#### PUT /ordem-servico/{id}/finalizar

Finaliza serviço

#### PUT /ordem-servico/{id}/entregar

Entrega veículo

---

### 5.8 Diagnóstico

#### PUT /diagnostico

Solicita diagnóstico

```json
{
  "ordemServicoId": "uuid"
}
```

#### POST /diagnostico

Registra diagnóstico

```json
{
  "ordemServicoId": "uuid",
  "valorEstimado": 0,
  "solucaoProposta": "string",
  "pecas": ["uuid"]
}
```

---

### 5.9 Orçamento

#### PUT /orcamento/{id}/aprovar

Aprova orçamento

#### PUT /orcamento/{id}/recusar

Recusa orçamento

```json
{
  "motivoRecusa": "string"
}
```

---

## 6. Passo-a-passo

A utilização da API segue um fluxo **sequencial e dependente de estado**, onde cada etapa habilita a próxima.

---

### 🔹 6.1. Inicialização do sistema

* Chamar o endpoint de autenticação com o usuário administrador (e-mail e senha fornecidos no GitHub).

Esse usuário já vem previamente configurado via variável de ambiente do Docker e possui permissões totais sobre o sistema.

---

### 🔹 6.2. Preparação de usuários

* Listar todos os perfis disponíveis para criação de usuário:

  ```
  GET /api/v1/perfil
  ```

* Criar um novo usuário:

  ```
  POST /api/v1/usuario
  ```

  Informando:

  * email
  * perfilId (obtido anteriormente)
  * senha (opcional)

⚠️ Regra importante:

* Caso a senha **não seja informada**, o usuário deverá realizar o fluxo de primeiro acesso.

---

### 🔹 6.3. Primeiro acesso (se necessário)

* Caso o usuário tenha sido criado sem senha:

  ```
  PUT /api/v1/usuario/primeiro-acesso
  ```

Após essa etapa, o usuário estará apto a autenticar normalmente.

---

### 🔹 6.4. Autenticação operacional

* Autenticar com o novo usuário criado **ou continuar com o administrador**.

A partir daqui, todas as operações exigem envio do **Bearer Token**.

---

### 🔹 6.5. Preparação do ambiente (estoque)

Antes de iniciar ordens de serviço, é necessário garantir que existam peças cadastradas:

* Cadastrar peças e insumos:

  ```
  POST /api/v1/peca
  ```

Essas peças serão utilizadas posteriormente no diagnóstico e execução da ordem de serviço.

---

### 🔹 6.6. Cadastro do cliente

* Cadastrar um novo cliente:

  ```
  POST /api/v1/cliente
  ```

📌 Importante:

* Armazene o `clienteId` retornado
* Caso necessário, o cliente pode ser recuperado posteriormente via listagem ou busca por documento

---

### 🔹 6.7. Cadastro do veículo

* Cadastrar veículo vinculado ao cliente:

  ```
  POST /api/v1/veiculo
  ```

Informando:

* clienteId (obrigatório)
* demais dados do veículo

📌 Armazene o `veiculoId`

---

### 🔹 6.8. Criação da Ordem de Serviço

* Criar uma nova ordem de serviço:

  ```
  POST /api/v1/ordem-servico
  ```

📌 Armazene o `ordemServicoId`

**Estado da ordem:**
➡️ Criada (aguardando diagnóstico)

---

### 🔹 6.9. Solicitação de diagnóstico

* Solicitar diagnóstico para a ordem:

  ```
  PUT /api/v1/diagnostico
  ```

**Estado da ordem:**
➡️ Em diagnóstico

---

### 🔹 6.10. Registro do diagnóstico

* Registrar o diagnóstico realizado pelo mecânico:

  ```
  POST /api/v1/diagnostico
  ```

Informando:

* valor estimado
* solução proposta
* peças necessárias

**Estado da ordem:**
➡️ Aguardando aprovação do cliente

---

### 🔹 6.11. Fluxo do cliente

Após o diagnóstico:

* O cliente já está cadastrado e possui acesso ao sistema
* O acesso é realizado utilizando o e-mail informado no cadastro

Caso seja o primeiro acesso:

* O cliente deverá definir sua senha via endpoint de primeiro acesso

---

### 🔹 6.12. Ação do cliente (decisão)

Após autenticação, o cliente poderá:

#### ✔ Aprovar o orçamento

```
PUT /api/v1/orcamento/{id}/aprovar
```

**Resultado:**
➡️ Ordem segue para execução

---

#### ❌ Recusar o orçamento

```
PUT /api/v1/orcamento/{id}/recusar
```

**Resultado:**
➡️ Ordem não será executada

---

### 🔹 6.13. Fluxo após decisão

#### ❌ Se recusado

* Registrar entrega do veículo:

  ```
  PUT /api/v1/ordem-servico/{id}/entregar
  ```

**Estado final:**
➡️ Encerrado sem execução

---

#### ✔ Se aprovado

A ordem segue para execução do serviço.

---

### 🔹 6.14. Execução do serviço

Durante a execução:

* As peças utilizadas podem ser baixadas do estoque:

  ```
  PUT /api/v1/peca/{id}/baixar
  ```

⚠️ Esse passo pode ser:

* manual (via API)
* ou automatizado dependendo da implementação futura

---

### 🔹 6.15. Finalização da ordem

Após conclusão do serviço:

```
PUT /api/v1/ordem-servico/{id}/finalizar
```

**Estado da ordem:**
➡️ Finalizada (aguardando retirada)

O sistema pode simular notificação ao cliente.

---

### 🔹 6.16. Entrega do veículo

Após retirada:

```
PUT /api/v1/ordem-servico/{id}/entregar
```

**Estado final:**
➡️ Concluída

---

#### 🔁 Resumo do fluxo

```
Criação da OS
 → Recebida
 → Em Diagnóstico
 → Aguardando aprovação
   → Recusado → Finalizado → Entregue
   → Aprovado
        → Em Execução
        → Finalizado
        → Entregue
```

---

## ⚠️ Observações importantes

* O fluxo **não é apenas CRUD**, ele representa um processo de negócio com estados
* A ordem correta das chamadas é responsabilidade do consumidor da API
* IDs retornados devem ser persistidos pelo cliente
* O sistema depende de autenticação contínua (token em todas requisições)
* Perfis influenciam permissões (admin, funcionário, cliente)
