using System.Security.Claims;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Aplication.Auth;

public interface ITokenService
{
    Task<AuthTokens> IssueTokensAsync(User user, AppDbContext context, CancellationToken cancellationToken = default);
    Task<AuthTokens?> RefreshAsync(string refreshToken, AppDbContext context, CancellationToken cancellationToken = default);
    ClaimsPrincipal? ValidateAccessToken(string accessToken);
    string HashRefreshToken(string refreshToken);
}
