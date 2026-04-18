using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class GetAllTodo
{
    public sealed record Request();

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("todoitem", Handler)
                .WithTags("Todo Item");
        }
    }

    public static async Task<IResult> Handler(AppDbContext context, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        ArgumentNullException.ThrowIfNull(context);

        List<TodoItemDto> todoItens =
         await context
            .Todos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ObterDetalhes()
            .ToListAsync();

        if (todoItens.Count == 0)
        {
            return Results.NoContent();
        }

        return Results.Ok(todoItens);
    }
}
