using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class CopyTodo
{
    public sealed record Response(Guid IdTodoItem, DateTime CreatedAt);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("todoitem/{id:guid}/copy", Handler)
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

        var newTodoItem = todoItem.Clone();

        await context.AddAsync(newTodoItem);
        await context.SaveChangesAsync();

        return Results.Ok(new Response(newTodoItem.Id, newTodoItem.CreatedAtUtc));
    }
}

