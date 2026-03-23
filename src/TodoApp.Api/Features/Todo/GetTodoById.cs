using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class GetTodoById
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("todoitem/{id:guid}", Handler)
                .WithTags("Todo Item");
        }
    }

    public static async Task<IResult> Handler(Guid id, AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        TodoItemDto? todoItem =
            await context
            .Todos
            .ObterDetalhes()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (todoItem is null)
        {
            return Results.NotFound(TodoItemError.NotFound);
        }

        return Results.Ok(todoItem);
    }
}
