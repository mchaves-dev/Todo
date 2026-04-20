# PRD Tecnico - Todo API

## 1. Visao Geral
API REST de gerenciamento de tarefas implementada com ASP.NET Core Minimal API.

Este documento descreve a implementacao tecnica. Para integracao com consumidores externos, a fonte oficial do contrato HTTP e o Swagger/OpenAPI gerado pela aplicacao.

## 2. Documentacao Swagger/OpenAPI
Em ambiente `Development`, a aplicacao publica:
- Swagger UI: `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`

Configuracao atual:
- Titulo: `Todo API`
- Versao: `v1`
- Descricao: API para cadastro, consulta, conclusao, arquivamento e copia de itens de tarefa.
- `CustomSchemaIds`: usa `FullName` com `+` substituido por `.`, evitando colisao entre records internos com mesmo nome, como `CreateTodo.Response` e `CopyTodo.Response`.

Os endpoints usam metadados OpenAPI diretamente no mapeamento Minimal API:
- `WithTags`
- `WithName`
- `WithSummary`
- `WithDescription`
- `Accepts`
- `Produces`
- `ProducesProblem`
- `ProducesValidationProblem`

## 3. Stack Tecnologica
- Plataforma: .NET 10 (`net10.0`)
- API: ASP.NET Core Minimal API
- Persistencia: Entity Framework Core 10
- Banco atual: `Microsoft.EntityFrameworkCore.InMemory` com database `mem`
- Documentacao: OpenAPI + Swagger (`Swashbuckle.AspNetCore`)
- Validacao: `FluentValidation`
- Resultado HTTP: `IResult`

## 4. Estrutura do Projeto
- Solucao: `src/Projetos.slnx`
- Projeto API: `src/TodoApp.Api`

Pastas principais:
- `Domain`: entidades, erros de dominio e enums
- `Infra/Database`: `AppDbContext`, interceptor de auditoria e mapeamentos EF Core
- `Aplication/Endpoints`: contrato `IEndpoint` e registro dinamico de endpoints
- `Aplication/Extensions`: extensoes de validacao e paginacao
- `Features/Todo`: endpoints organizados por caso de uso
- `Features/Todo/SharedTodo`: rotas, DTOs e queries compartilhadas

## 5. Registro de Endpoints
O projeto usa endpoints por feature:
- Cada endpoint implementa `IEndpoint`.
- `AddEndpoints()` registra dinamicamente implementacoes encontradas no assembly.
- `MapEndpoints()` percorre os endpoints registrados e chama `MapEndpoint()`.

Essa abordagem mantem o contrato HTTP proximo ao caso de uso.

## 6. Rotas e Versionamento
Base versionada:
- `/api/v1/todoitems`

Rotas atuais:
- `POST /api/v1/todoitems`
- `GET /api/v1/todoitems`
- `GET /api/v1/todoitems/{id}`
- `PATCH /api/v1/todoitems/{id}/completed`
- `PATCH /api/v1/todoitems/{id}/archived`
- `POST /api/v1/todoitems/{id}/copy`

Tag Swagger compartilhada:
- `Todo Item`

## 7. Contratos HTTP

### 7.1 Sucesso
- Criacao e copia retornam `201 Created` com body `{ idTodoItem, createdAt }` e header `Location`.
- Consulta por id retorna `200 OK` com `TodoItemDto`.
- Listagem retorna `200 OK` com lista de `TodoItemDto`.
- Concluir e arquivar retornam `204 NoContent`.

### 7.2 Erros
- Entrada invalida retorna `400 BadRequest` com `HttpValidationProblemDetails`.
- Paginacao invalida retorna `400 BadRequest` com `HttpValidationProblemDetails`.
- Item inexistente retorna `404 NotFound` com `ProblemDetails`.

Mensagens atuais:
- Nao encontrado: `Item de tarefa nao encontrado.`
- Paginacao invalida: `deve ser maior que zero.`

## 8. Validacao

### 8.1 Criacao
Validador `CreateTodo.Validator`:
- `userId`: obrigatorio
- `description`: obrigatorio, tamanho entre 2 e 200
- `priority`: deve ser valor valido do enum

### 8.2 Paginacao
Validacao centralizada em `PaginationExtensions.ValidatePagination(page, pageSize)`:
- `page > 0`
- `pageSize > 0`

Uso atual:
- `GetAllTodo`

## 9. Modelo de Dados
Entidade `TodoItem`:
- `Id`
- `UserId`
- `Description`
- `DueDate`
- `Labels`
- `IsCompleted`
- `CompletedAt`
- `Priority`
- `IsArchived`
- `ArchivedAt`
- `CreatedAtUtc`
- `UpdatedAtUtc`

DTO de leitura `TodoItemDto` exposto no Swagger:
- `Id`
- `UserId`
- `Description`
- `DueDate`
- `Labels`
- `IsCompleted`
- `CompletedAt`
- `Priority`
- `IsArchived`
- `ArchivedAt`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Enum `EPriority`:
- `None = 0`
- `High = 1`
- `Medium = 2`
- `Low = 3`

## 10. Auditoria e Persistencia
- `AuditableInterceptor` e registrado como singleton.
- O `DbContext` usa banco em memoria chamado `mem`.
- Dados nao persistem entre reinicios da aplicacao.

## 11. Observacoes Tecnicas
- O Swagger e habilitado apenas em ambiente `Development`.
- Nao ha autenticacao/autorizacao no estado atual.
- Os schemas do Swagger usam nomes completos para evitar conflito entre tipos aninhados com o mesmo nome.
- Os endpoints de mudanca de estado sao idempotentes pela regra da entidade.
