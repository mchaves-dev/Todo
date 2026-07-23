using Microsoft.AspNetCore.SignalR;

namespace TodoApp.Api.Features.Todo.Realtime;

public sealed class TodoRealtimeNotifier : ITodoRealtimeNotifier
{
    private readonly IHubContext<TodoHub> hubContext;

    public TodoRealtimeNotifier(IHubContext<TodoHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public Task NotifyChangedAsync(CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.All.SendCoreAsync("TodoItemsChanged", [], cancellationToken);
    }
}
