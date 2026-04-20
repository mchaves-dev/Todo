using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class ArchivedTodo
{
    public sealed record Request(Guid id);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch($"{TodoRoutes.Base}/{{id:guid}}/archived", Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("ArchiveTodoItem")
                .WithSummary("Arquiva um item de tarefa")
                .WithDescription("Atualiza o item para arquivado e registra a data de arquivamento em UTC.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }

    internal static async Task<IResult> Handler(Guid id, AppDbContext context)
    {
        var todoItem = await context.Todos.FindAsync(id);

        if (todoItem is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item de tarefa nao encontrado",
                detail: TodoItemError.NotFound);
        }

        todoItem.Archived();

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}

