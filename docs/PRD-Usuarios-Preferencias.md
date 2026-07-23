# PRD - Usuarios e Preferencias

## 1. Objetivo
Adicionar suporte a usuarios e preferencias pessoais na Todo API.

Nesta etapa, o escopo nao inclui multi-tenant, projetos ou controle granular de acessos. Autenticacao com token e refresh token foi adicionada em documento proprio.

## 2. Contexto
A API atual permite criar e consultar tarefas usando `userId` informado no corpo da requisicao.

Com a criacao formal de usuarios, o sistema passa a ter uma entidade propria para representar quem cria e gerencia tarefas. As preferencias permitem personalizar a experiencia do usuario sem alterar a estrutura das tarefas.

## 3. Escopo

### Incluido
- Cadastro de usuarios.
- Cadastro de senha do usuario.
- Consulta de usuarios.
- Consulta de usuario por id.
- Atualizacao de dados basicos do usuario.
- Cadastro e atualizacao de preferencias do usuario.
- Consulta das preferencias do usuario.
- Associacao das tarefas existentes a um usuario cadastrado.

### Fora de escopo
- Multi-tenant.
- Perfis de acesso e permissoes.
- Projetos.
- Convites de usuario.
- Exclusao fisica de usuario.
- Recuperacao de conta.

## 4. Entidades

### 4.1 User
Representa um usuario do sistema.

Campos:
- `Id`: identificador unico.
- `Name`: nome do usuario.
- `Email`: e-mail unico.
- `PasswordHash`: hash da senha do usuario.
- `IsActive`: indica se o usuario esta ativo.
- `CreatedAtUtc`: data de criacao em UTC.
- `UpdatedAtUtc`: data da ultima atualizacao em UTC.

Regras:
- `Name` e obrigatorio.
- `Email` e obrigatorio.
- `Email` deve ter formato valido.
- `Email` deve ser unico.
- Senha e obrigatoria no cadastro e armazenada apenas como hash.
- Usuario nasce ativo.
- Usuario inativo nao deve criar novas tarefas.

### 4.2 UserPreference
Representa preferencias pessoais do usuario.

Campos:
- `Id`: identificador unico.
- `UserId`: usuario dono das preferencias.
- `Theme`: tema visual preferido.
- `Language`: idioma preferido.
- `Timezone`: fuso horario preferido.
- `DefaultPageSize`: tamanho padrao de pagina nas listagens.
- `ShowArchived`: indica se tarefas arquivadas devem aparecer por padrao.
- `CreatedAtUtc`: data de criacao em UTC.
- `UpdatedAtUtc`: data da ultima atualizacao em UTC.

Regras:
- Cada usuario possui no maximo um registro de preferencias.
- Ao criar um usuario, o sistema deve criar preferencias padrao.
- Preferencias podem ser atualizadas parcialmente.
- `DefaultPageSize` deve ser maior que zero.

Valores padrao:
- `Theme`: `System`
- `Language`: `pt-BR`
- `Timezone`: `America/Sao_Paulo`
- `DefaultPageSize`: `5`
- `ShowArchived`: `false`

## 5. Contratos HTTP

Base de rotas:
- `/api/v1/users`

### 5.1 Criar usuario
- Endpoint: `POST /api/v1/users`
- Objetivo: criar um usuario ativo com preferencias padrao.

Request:
- `name`: obrigatorio.
- `email`: obrigatorio.
- `password`: obrigatoria, 8 a 100 caracteres.

Respostas:
- `201 Created` com `{ idUser, createdAt }`.
- `400 BadRequest` para entrada invalida.
- `409 Conflict` quando o e-mail ja existir.

### 5.2 Listar usuarios
- Endpoint: `GET /api/v1/users?page={n}&pageSize={n}`
- Objetivo: listar usuarios cadastrados.

Query:
- `page`: opcional, default `1`.
- `pageSize`: opcional, default `5`.

Respostas:
- `200 OK` com lista de usuarios.
- `400 BadRequest` quando a paginacao for invalida.

### 5.3 Obter usuario por id
- Endpoint: `GET /api/v1/users/{id}`
- Objetivo: consultar dados de um usuario.

Respostas:
- `200 OK` com dados do usuario.
- `404 NotFound` quando o usuario nao existir.

### 5.4 Atualizar usuario
- Endpoint: `PATCH /api/v1/users/{id}`
- Objetivo: atualizar dados basicos do usuario.

Request:
- `name`: opcional.
- `email`: opcional.
- `isActive`: opcional.

Respostas:
- `204 NoContent`.
- `400 BadRequest` para entrada invalida.
- `404 NotFound` quando o usuario nao existir.
- `409 Conflict` quando o novo e-mail ja estiver em uso.

### 5.5 Obter preferencias
- Endpoint: `GET /api/v1/users/{id}/preferences`
- Objetivo: consultar preferencias do usuario.

Respostas:
- `200 OK` com preferencias.
- `404 NotFound` quando o usuario nao existir.

### 5.6 Atualizar preferencias
- Endpoint: `PATCH /api/v1/users/{id}/preferences`
- Objetivo: atualizar preferencias do usuario.

Request:
- `theme`: opcional.
- `language`: opcional.
- `timezone`: opcional.
- `defaultPageSize`: opcional.
- `showArchived`: opcional.

Respostas:
- `204 NoContent`.
- `400 BadRequest` para entrada invalida.
- `404 NotFound` quando o usuario nao existir.

## 6. DTOs

### 6.1 UserDto
Campos:
- `id`
- `name`
- `email`
- `isActive`
- `createdAtUtc`
- `updatedAtUtc`

### 6.2 UserPreferenceDto
Campos:
- `id`
- `userId`
- `theme`
- `language`
- `timezone`
- `defaultPageSize`
- `showArchived`
- `createdAtUtc`
- `updatedAtUtc`

## 7. Impacto Em Tarefas
A criacao de tarefas deve validar se o `userId` informado existe e esta ativo.

Regras:
- Se o usuario nao existir, retornar `400 BadRequest`.
- Se o usuario estiver inativo, retornar `400 BadRequest`.
- Listagens futuras poderao usar `DefaultPageSize` quando o consumidor nao informar `pageSize`.

## 8. Validacoes

Usuario:
- `name`: obrigatorio, 2 a 120 caracteres.
- `email`: obrigatorio, formato valido, maximo 180 caracteres.
- `password`: obrigatoria no cadastro, 8 a 100 caracteres.

Preferencias:
- `theme`: valores aceitos: `System`, `Light`, `Dark`.
- `language`: obrigatorio quando informado, maximo 10 caracteres.
- `timezone`: obrigatorio quando informado, maximo 80 caracteres.
- `defaultPageSize`: maior que zero e no maximo 100.

## 9. Persistencia
Adicionar ao `AppDbContext`:
- `DbSet<User>`
- `DbSet<UserPreference>`
- `DbSet<RefreshToken>`

Mapeamentos EF Core:
- indice unico para `User.Email`.
- `User.PasswordHash` obrigatorio.
- relacionamento `User` 1:1 `UserPreference`.
- relacionamento `User` 1:N `TodoItem`.
- relacionamento `User` 1:N `RefreshToken`.

## 10. Swagger/OpenAPI
Todos os novos endpoints devem conter:
- `WithTags("User")`
- `WithName`
- `WithSummary`
- `WithDescription`
- `Produces`
- `ProducesProblem`
- `ProducesValidationProblem`

## 11. Criterios De Aceite
- Deve ser possivel criar usuario com preferencias padrao.
- Deve ser possivel criar usuario com senha.
- Nao deve ser possivel criar dois usuarios com o mesmo e-mail.
- Deve ser possivel listar usuarios com paginacao.
- Deve ser possivel consultar usuario por id.
- Deve ser possivel atualizar nome, e-mail e status ativo.
- Deve ser possivel consultar preferencias de um usuario.
- Deve ser possivel atualizar preferencias parcialmente.
- Tarefa nova deve exigir usuario existente e ativo.
- Swagger deve documentar todos os contratos novos.
