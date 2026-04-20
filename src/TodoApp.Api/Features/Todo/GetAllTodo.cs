using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Aplication.Extensions;
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
            app.MapGet(TodoRoutes.Base, Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("GetTodoItems")
                .WithSummary("Lista itens de tarefa")
                .WithDescription("""
                    Retorna uma pagina de itens de tarefa.
                    Use page e pageSize para controlar a paginacao. Ambos devem ser maiores que zero.
                    """)
                .Produces<List<TodoItemDto>>(StatusCodes.Status200OK)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<IResult> Handler(AppDbContext context, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        ArgumentNullException.ThrowIfNull(context);

        IDictionary<string, string[]>? paginationErrors = PaginationExtensions.ValidatePagination(page, pageSize);

        if (paginationErrors is not null)
        {
            return Results.ValidationProblem(paginationErrors);
        }

        List<TodoItemDto> todoItens =
         await context
            .Todos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ObterDetalhes()
            .ToListAsync();

        return Results.Ok(todoItens);
    }
}
