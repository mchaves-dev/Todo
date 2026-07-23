# PRD Funcional - Funcionalidades Existentes

## 1. Objetivo
Disponibilizar uma API de tarefas (Todo) para criacao, consulta, alteracao de estado e copia de itens.

Este documento e apoio funcional. A documentacao de integracao oficial para desenvolvedores consumidores da API e o Swagger/OpenAPI exposto pela aplicacao.

## 2. Documentacao de Integracao
Em ambiente `Development`, acessar:
- Swagger UI: `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`

O Swagger contem:
- Rotas disponiveis
- Parametros de rota, query e body
- Schemas de request/response
- Codigos HTTP de sucesso e erro
- Descricoes funcionais por endpoint

## 3. Base de Rotas
Todos os endpoints atuais usam base versionada:
- `/api/v1/todoitems`
- `/api/v1/users`
- `/api/v1/auth`

## 4. Funcionalidades Implementadas

### 4.0 Autenticacao
- Endpoint: `POST /api/v1/auth/login`
- Objetivo: autenticar usuario ativo por e-mail e senha.
- Saidas:
  - `200 OK` com `accessToken`, `refreshToken`, expiracoes e `tokenType`
  - `400 BadRequest` para entrada invalida
  - `401 Unauthorized` para credenciais invalidas

- Endpoint: `POST /api/v1/auth/refresh`
- Objetivo: renovar o par de tokens usando refresh token ativo.
- Saidas:
  - `200 OK` com novo par de tokens
  - `400 BadRequest` para entrada invalida
  - `401 Unauthorized` para refresh token invalido, expirado ou revogado

As rotas de tarefas exigem `Authorization: Bearer {accessToken}`.

### 4.1 Criar tarefa
- Endpoint: `POST /api/v1/todoitems`
- Nome no Swagger: `CreateTodoItem`
- Objetivo: criar um novo item de tarefa para um usuario.
- Entrada JSON:
  - `userId`: obrigatorio, `Guid`
  - `description`: obrigatorio, 2 a 200 caracteres
  - `priority`: obrigatorio, valor valido de `EPriority`
  - `dueDate`: opcional, `DateTime`
  - `labels`: opcional, array de strings
- Saidas:
  - `201 Created` com `{ idTodoItem, createdAt }`
  - Header `Location` apontando para o recurso criado
  - `400 BadRequest` com `HttpValidationProblemDetails` para entrada invalida

### 4.2 Listar tarefas
- Endpoint: `GET /api/v1/todoitems?page={n}&pageSize={n}`
- Nome no Swagger: `GetTodoItems`
- Objetivo: retornar uma pagina de itens de tarefa.
- Parametros de query:
  - `page`: opcional, default `1`, deve ser maior que zero
  - `pageSize`: opcional, default `5`, deve ser maior que zero
- Saidas:
  - `200 OK` com lista de `TodoItemDto`
  - `400 BadRequest` com `HttpValidationProblemDetails` quando a paginacao for invalida

### 4.3 Obter tarefa por id
- Endpoint: `GET /api/v1/todoitems/{id}`
- Nome no Swagger: `GetTodoItemById`
- Objetivo: retornar os detalhes de um item de tarefa pelo identificador.
- Parametros:
  - `id`: obrigatorio, `Guid`
- Saidas:
  - `200 OK` com `TodoItemDto`
  - `404 NotFound` com `ProblemDetails` quando o item nao existir

### 4.4 Marcar tarefa como concluida
- Endpoint: `PATCH /api/v1/todoitems/{id}/completed`
- Nome no Swagger: `CompleteTodoItem`
- Objetivo: marcar o item como concluido e preencher `completedAt` em UTC.
- Parametros:
  - `id`: obrigatorio, `Guid`
- Saidas:
  - `204 NoContent` em sucesso
  - `404 NotFound` com `ProblemDetails` quando o item nao existir

### 4.5 Arquivar tarefa
- Endpoint: `PATCH /api/v1/todoitems/{id}/archived`
- Nome no Swagger: `ArchiveTodoItem`
- Objetivo: marcar o item como arquivado e preencher `archivedAt` em UTC.
- Parametros:
  - `id`: obrigatorio, `Guid`
- Saidas:
  - `204 NoContent` em sucesso
  - `404 NotFound` com `ProblemDetails` quando o item nao existir

### 4.6 Copiar tarefa
- Endpoint: `POST /api/v1/todoitems/{id}/copy`
- Nome no Swagger: `CopyTodoItem`
- Objetivo: criar uma nova tarefa copiando usuario, descricao, prioridade, vencimento e labels da tarefa informada.
- Parametros:
  - `id`: obrigatorio, `Guid`
- Saidas:
  - `201 Created` com `{ idTodoItem, createdAt }`
  - Header `Location` apontando para a copia criada
  - `404 NotFound` com `ProblemDetails` quando o item original nao existir

## 5. Contratos de Dados

### 5.1 EPriority
Valores atuais:
- `0`: `None`
- `1`: `High`
- `2`: `Medium`
- `3`: `Low`

### 5.2 TodoItemDto
Campos retornados nas consultas:
- `id`
- `userId`
- `description`
- `dueDate`
- `labels`
- `isCompleted`
- `completedAt`
- `priority`
- `isArchived`
- `archivedAt`
- `createdAtUtc`
- `updatedAtUtc`

## 6. Regras Funcionais Atuais
- A tarefa nasce nao concluida e nao arquivada.
- A conclusao e idempotente: se ja estiver concluida, nao altera novamente.
- O arquivamento e idempotente: se ja estiver arquivada, nao altera novamente.
- A copia nasce como nova tarefa, com novo `id` e nova data de criacao.
- Erros de nao encontrado usam mensagem padronizada em PT-BR.

## 7. Limitacoes Atuais
- Sem edicao de descricao, prazo, labels ou prioridade.
- Sem exclusao de tarefa.
- Sem filtros por status, prioridade, labels ou usuario.
- Sem ordenacao configuravel na listagem.
- Autenticacao implementada, mas ainda sem perfis de acesso ou permissoes granulares.
- Persistencia atual em memoria; os dados nao sobrevivem ao reinicio da aplicacao.
