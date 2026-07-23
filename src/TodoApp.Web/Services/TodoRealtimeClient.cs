using Microsoft.AspNetCore.SignalR.Client;

namespace TodoApp.Web.Services;

public sealed class TodoRealtimeClient : IAsyncDisposable
{
    private readonly HttpClient httpClient;
    private HubConnection? connection;

    public TodoRealtimeClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public event EventHandler? TodoItemsChanged;

    public bool IsConnected => connection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        if (connection is not null)
        {
            return;
        }

        Uri hubUri = new(httpClient.BaseAddress!, "/hubs/todos");

        connection = new HubConnectionBuilder()
            .WithUrl(hubUri)
            .WithAutomaticReconnect()
            .Build();

        connection.On("TodoItemsChanged", () => TodoItemsChanged?.Invoke(this, EventArgs.Empty));

        try
        {
            await connection.StartAsync();
        }
        catch (HttpRequestException)
        {
            await DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is null)
        {
            return;
        }

        await connection.DisposeAsync();
        connection = null;
    }
}
