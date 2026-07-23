# PRD - Autenticacao

## 1. Objetivo
Adicionar autenticacao na Todo API com token de acesso e refresh token.

## 2. Escopo

### Incluido
- Cadastro de usuario com senha.
- Hash de senha com PBKDF2-SHA256.
- Login por e-mail e senha.
- Emissao de token de acesso Bearer.
- Emissao de refresh token.
- Renovacao de tokens usando refresh token ativo.
- Rotacao de refresh token: o token usado e revogado e um novo refresh token e emitido.
- Protecao das rotas de tarefas e das rotas de consulta/atualizacao de usuarios.

### Fora de escopo
- Recuperacao de senha.
- Confirmacao de e-mail.
- MFA.
- Perfis e permissoes granulares.
- Logout global ou revogacao de todos os dispositivos.

## 3. Configuracao
Secao `Auth` em `appsettings`:
- `TokenSecret`: segredo usado para assinar o token de acesso. Deve ter pelo menos 32 caracteres.
- `AccessTokenMinutes`: duracao do token de acesso em minutos. Valor atual: `15`.
- `RefreshTokenDays`: duracao do refresh token em dias. Valor atual: `7`.

Em `appsettings.json`, `TokenSecret` fica vazio por seguranca. Ambientes reais devem informar o segredo por configuracao segura. O ambiente `Development` possui um segredo local apenas para execucao de desenvolvimento.

## 4. Contratos HTTP

Base de rotas:
- `/api/v1/auth`

### 4.1 Login
- Endpoint: `POST /api/v1/auth/login`
- Objetivo: autenticar usuario ativo por e-mail e senha.

Request:
- `email`: obrigatorio, formato de e-mail.
- `password`: obrigatoria.

Respostas:
- `200 OK` com `AuthTokens`.
- `400 BadRequest` para entrada invalida.
- `401 Unauthorized` para credenciais invalidas, usuario inexistente ou usuario inativo.

### 4.2 Renovar tokens
- Endpoint: `POST /api/v1/auth/refresh`
- Objetivo: renovar o par de tokens usando refresh token ativo.

Request:
- `refreshToken`: obrigatorio.

Respostas:
- `200 OK` com novo `AuthTokens`.
- `400 BadRequest` para entrada invalida.
- `401 Unauthorized` para refresh token invalido, expirado ou revogado.

## 5. DTOs

### 5.1 AuthTokens
Campos:
- `accessToken`
- `accessTokenExpiresAtUtc`
- `refreshToken`
- `refreshTokenExpiresAtUtc`
- `tokenType`: sempre `Bearer`

## 6. Regras
- O token de acesso deve ser enviado no header `Authorization: Bearer {accessToken}`.
- Refresh tokens sao armazenados apenas como hash SHA-256.
- Refresh tokens possuem expiracao e data de revogacao.
- Ao usar um refresh token com sucesso, ele e revogado e substituido por outro.
- Apenas usuarios ativos podem autenticar ou renovar tokens.
- Cadastro de usuario exige senha entre 8 e 100 caracteres.
