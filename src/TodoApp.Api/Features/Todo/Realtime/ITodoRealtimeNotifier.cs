namespace TodoApp.Api.Features.Todo.Realtime;

public interface ITodoRealtimeNotifier
{
    Task NotifyChangedAsync(CancellationToken cancellationToken = default);
}

