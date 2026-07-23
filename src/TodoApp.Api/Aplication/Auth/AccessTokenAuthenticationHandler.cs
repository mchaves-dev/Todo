using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TodoApp.Api.Aplication.Auth;

public sealed class AccessTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ITokenService tokenService;

    public AccessTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokenService) : base(options, logger, encoder)
    {
        this.tokenService = tokenService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorization = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        const string bearerPrefix = "Bearer ";

        if (!authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization deve usar Bearer token."));
        }

        string accessToken = authorization[bearerPrefix.Length..].Trim();
        var principal = tokenService.ValidateAccessToken(accessToken);

        if (principal is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token invalido ou expirado."));
        }

        var ticket = new AuthenticationTicket(principal, AuthConstants.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
