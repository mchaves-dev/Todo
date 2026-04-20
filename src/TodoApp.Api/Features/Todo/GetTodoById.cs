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
            app.MapGet($"{TodoRoutes.Base}/{{id:guid}}", Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("GetTodoItemById")
                .WithSummary("Consulta um item de tarefa")
                .WithDescription("Retorna os detalhes de um item de tarefa pelo identificador.")
                .Produces<TodoItemDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);
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
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Item de tarefa nao encontrado",
                detail: TodoItemError.NotFound);
        }

        return Results.Ok(todoItem);
    }
}

