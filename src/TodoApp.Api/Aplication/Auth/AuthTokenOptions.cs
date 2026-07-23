namespace TodoApp.Api.Aplication.Auth;

public sealed class AuthTokenOptions
{
    public const string SectionName = "Auth";

    public string TokenSecret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
