using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TodoApp.Web.Models;

namespace TodoApp.Web.Services;

public sealed class TodoApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient httpClient;

    public TodoApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AuthTokens> LoginAsync(string email, string password)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password), JsonOptions);
        await EnsureSuccessAsync(response, "Nao foi possivel entrar.");

        return await response.Content.ReadFromJsonAsync<AuthTokens>(JsonOptions) ?? throw new InvalidOperationException("Resposta de autenticacao vazia.");
    }

    public async Task<CreateUserResponse> CreateUserAsync(string name, string email, string password)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/v1/users", new CreateUserRequest(name, email, password), JsonOptions);
        await EnsureSuccessAsync(response, "Nao foi possivel criar a conta.");

        return await response.Content.ReadFromJsonAsync<CreateUserResponse>(JsonOptions) ?? throw new InvalidOperationException("Resposta de usuario vazia.");
    }

    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync(int page = 1, int pageSize = 50)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<TodoItem>>($"/api/v1/todoitems?page={page}&pageSize={pageSize}", JsonOptions) ?? [];
    }

    public async Task<UserPreference?> GetPreferencesAsync(Guid userId)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(new Uri($"/api/v1/users/{userId}/preferences", UriKind.Relative));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "Nao foi possivel carregar as preferencias.");

        return await response.Content.ReadFromJsonAsync<UserPreference>(JsonOptions);
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdateUserPreferenceRequest request)
    {
        using HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"/api/v1/users/{userId}/preferences", request, JsonOptions);
        await EnsureSuccessAsync(response, "Nao foi possivel atualizar as preferencias.");
    }

    public async Task<CreateTodoResponse> CreateTodoAsync(CreateTodoRequest request)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/v1/todoitems", request, JsonOptions);
        await EnsureSuccessAsync(response, "Nao foi possivel criar a tarefa.");

        return await response.Content.ReadFromJsonAsync<CreateTodoResponse>(JsonOptions) ?? throw new InvalidOperationException("Resposta de tarefa vazia.");
    }

    public async Task CompleteTodoAsync(Guid id)
    {
        using HttpResponseMessage response = await httpClient.PatchAsync(new Uri($"/api/v1/todoitems/{id}/completed", UriKind.Relative), null);
        await EnsureSuccessAsync(response, "Nao foi possivel concluir a tarefa.");
    }

    public async Task ArchiveTodoAsync(Guid id)
    {
        using HttpResponseMessage response = await httpClient.PatchAsync(new Uri($"/api/v1/todoitems/{id}/archived", UriKind.Relative), null);
        await EnsureSuccessAsync(response, "Nao foi possivel arquivar a tarefa.");
    }

    public async Task CopyTodoAsync(Guid id)
    {
        using HttpResponseMessage response = await httpClient.PostAsync(new Uri($"/api/v1/todoitems/{id}/copy", UriKind.Relative), null);
        await EnsureSuccessAsync(response, "Nao foi possivel copiar a tarefa.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallbackMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string detail = fallbackMessage;
        string content = await response.Content.ReadAsStringAsync();

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(content);

                if (document.RootElement.TryGetProperty("detail", out JsonElement detailElement))
                {
                    detail = detailElement.GetString() ?? detail;
                }
                else if (document.RootElement.TryGetProperty("title", out JsonElement titleElement))
                {
                    detail = titleElement.GetString() ?? detail;
                }
                else if (document.RootElement.TryGetProperty("errors", out JsonElement errorsElement))
                {
                    detail = string.Join(" ", errorsElement.EnumerateObject().SelectMany(error => error.Value.EnumerateArray().Select(message => message.GetString())));
                }
            }
            catch (JsonException)
            {
                detail = content;
            }
        }

        throw new InvalidOperationException(detail);
    }
}
