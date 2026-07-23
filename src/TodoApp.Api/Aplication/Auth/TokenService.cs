using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Aplication.Auth;

public sealed class TokenService : ITokenService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly AuthTokenOptions options;
    private readonly TimeProvider timeProvider;

    public TokenService(IOptions<AuthTokenOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        this.options = options.Value;
        this.timeProvider = timeProvider;
    }

    public async Task<AuthTokens> IssueTokensAsync(User user, AppDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(context);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        DateTime accessTokenExpiresAtUtc = utcNow.AddMinutes(options.AccessTokenMinutes);
        DateTime refreshTokenExpiresAtUtc = utcNow.AddDays(options.RefreshTokenDays);

        string accessToken = CreateAccessToken(user, accessTokenExpiresAtUtc);
        string refreshToken = CreateRefreshToken();

        await context.RefreshTokens.AddAsync(
            new RefreshToken(user.Id, HashRefreshToken(refreshToken), refreshTokenExpiresAtUtc),
            cancellationToken);

        return new AuthTokens(accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc);
    }

    public async Task<AuthTokens?> RefreshAsync(string refreshToken, AppDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentNullException.ThrowIfNull(context);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        string refreshTokenHash = HashRefreshToken(refreshToken);

        RefreshToken? storedToken = await context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive(utcNow) || !storedToken.User.IsActive)
        {
            return null;
        }

        storedToken.Revoke(utcNow);

        return await IssueTokensAsync(storedToken.User, context, cancellationToken);
    }

    public ClaimsPrincipal? ValidateAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        string[] parts = accessToken.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        if (!IsValidSignature(parts[0], parts[1]))
        {
            return null;
        }

        if (!TryReadPayload(parts[0], out AccessTokenPayload payload))
        {
            return null;
        }

        DateTime expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnixTime).UtcDateTime;

        if (expiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            return null;
        }

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
            new(ClaimTypes.Email, payload.Email),
            new(ClaimTypes.Name, payload.Name),
            new("jti", payload.TokenId)
        ];

        var identity = new ClaimsIdentity(claims, AuthConstants.Scheme);

        return new ClaimsPrincipal(identity);
    }

    private bool IsValidSignature(string encodedPayload, string encodedSignature)
    {
        byte[] signature;

        try
        {
            signature = Base64UrlDecode(encodedSignature);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] expectedSignature = Sign(encodedPayload);

        return CryptographicOperations.FixedTimeEquals(signature, expectedSignature);
    }

    private static bool TryReadPayload(string encodedPayload, out AccessTokenPayload payload)
    {
        try
        {
            AccessTokenPayload? deserializedPayload = JsonSerializer.Deserialize<AccessTokenPayload>(
                Encoding.UTF8.GetString(Base64UrlDecode(encodedPayload)),
                SerializerOptions);

            if (deserializedPayload is null)
            {
                payload = null!;

                return false;
            }

            payload = deserializedPayload;

            return true;
        }
        catch (JsonException)
        {
            payload = null!;

            return false;
        }
        catch (FormatException)
        {
            payload = null!;

            return false;
        }
    }

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(hash);
    }

    private string CreateAccessToken(User user, DateTime expiresAtUtc)
    {
        EnsureValidSecret();

        var payload = new AccessTokenPayload(
            user.Id,
            user.Email,
            user.Name,
            new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds(),
            Guid.NewGuid().ToString("N"));

        string payloadText = JsonSerializer.Serialize(payload, SerializerOptions);
        string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadText));
        string encodedSignature = Base64UrlEncode(Sign(encodedPayload));

        return $"{encodedPayload}.{encodedSignature}";
    }

    private static string CreateRefreshToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    }

    private byte[] Sign(string encodedPayload)
    {
        EnsureValidSecret();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.TokenSecret));

        return hmac.ComputeHash(Encoding.UTF8.GetBytes(encodedPayload));
    }

    private void EnsureValidSecret()
    {
        if (string.IsNullOrWhiteSpace(options.TokenSecret) || options.TokenSecret.Length < 32)
        {
            throw new InvalidOperationException("Auth:TokenSecret deve ter pelo menos 32 caracteres.");
        }
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');
        int padding = base64.Length % 4;

        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + 4 - padding, '=');
        }

        return Convert.FromBase64String(base64);
    }

    private sealed record AccessTokenPayload(
        Guid UserId,
        string Email,
        string Name,
        long ExpiresAtUnixTime,
        string TokenId);
}
