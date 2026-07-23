using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp.Web.Models;

namespace TodoApp.Web.Services;

public sealed class AuthSession
{
    private const string StorageKey = "todo-flow.auth";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient httpClient;
    private readonly IJSRuntime jsRuntime;

    public AuthSession(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        this.httpClient = httpClient;
        this.jsRuntime = jsRuntime;
    }

    public event EventHandler? Changed;

    public AuthTokens? Tokens { get; private set; }

    public UserProfile? User { get; private set; }

    public bool IsAuthenticated => Tokens is not null && User is not null && Tokens.AccessTokenExpiresAtUtc > DateTime.UtcNow;

    public async Task InitializeAsync()
    {
        string? storedValue = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return;
        }

        AuthTokens? tokens = JsonSerializer.Deserialize<AuthTokens>(storedValue, JsonOptions);

        if (tokens is null || tokens.AccessTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            await SignOutAsync();
            return;
        }

        SetTokens(tokens);
    }

    public async Task SignInAsync(AuthTokens tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        SetTokens(tokens);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(tokens, JsonOptions));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task SignOutAsync()
    {
        Tokens = null;
        User = null;
        httpClient.DefaultRequestHeaders.Authorization = null;
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void SetTokens(AuthTokens tokens)
    {
        Tokens = tokens;
        User = ReadUser(tokens.AccessToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokens.TokenType, tokens.AccessToken);
    }

    private static UserProfile? ReadUser(string accessToken)
    {
        string[] parts = accessToken.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        try
        {
            string payload = parts[0].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using JsonDocument document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            JsonElement root = document.RootElement;

            return new UserProfile(
                root.GetProperty("userId").GetGuid(),
                root.GetProperty("name").GetString() ?? "Usuario",
                root.GetProperty("email").GetString() ?? string.Empty);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
