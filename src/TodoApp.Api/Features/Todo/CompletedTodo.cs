using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class CompletedTodo
{
    public sealed record Request(Guid id);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("todoitem/{id:guid}/completed", Handler)
                .WithTags("Todo Item");
        }
    }

    internal static async Task<IResult> Handler(Guid id, AppDbContext context)
    {
        var todoItem = await context.Todos.FindAsync(id);

        if (todoItem is null)
        {
            return Results.BadRequest(TodoItemError.NotFound);
        }

        todoItem.Compreted();

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
