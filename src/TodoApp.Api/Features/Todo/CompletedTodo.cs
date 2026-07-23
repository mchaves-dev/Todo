using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Features.Todo.Realtime;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class CompletedTodo
{
    public sealed record Request(Guid id);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch($"{TodoRoutes.Base}/{{id:guid}}/completed", Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("CompleteTodoItem")
                .WithSummary("Marca um item de tarefa como concluido")
                .WithDescription("Atualiza o item para concluido e registra a data de conclusao em UTC.")
                .RequireAuthorization()
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }

    internal static async Task<IResult> Handler(Guid id, AppDbContext context, ITodoRealtimeNotifier realtimeNotifier)
    {
        var todoItem = await context.Todos.FindAsync(id);

        if (todoItem is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item de tarefa nao encontrado",
                detail: TodoItemError.NotFound);
        }

        todoItem.Complete();

        await context.SaveChangesAsync();
        await realtimeNotifier.NotifyChangedAsync();

        return Results.NoContent();
    }
}

