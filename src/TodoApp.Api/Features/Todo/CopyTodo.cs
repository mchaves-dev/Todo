using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class CopyTodo
{
    public sealed record Response(Guid IdTodoItem, DateTime CreatedAt);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost($"{TodoRoutes.Base}/{{id:guid}}/copy", Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("CopyTodoItem")
                .WithSummary("Copia um item de tarefa")
                .WithDescription("Cria um novo item copiando usuario, descricao, prioridade, vencimento e labels do item informado.")
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
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

        var user = await context.Users.FindAsync(todoItem.UserId);

        if (user is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["userId"] = ["usuario nao encontrado."]
            });
        }

        if (!user.IsActive)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["userId"] = ["usuario inativo nao pode criar tarefas."]
            });
        }

        var newTodoItem = todoItem.Clone();

        await context.AddAsync(newTodoItem);
        await context.SaveChangesAsync();

        return Results.Created($"{TodoRoutes.Base}/{newTodoItem.Id}", new Response(newTodoItem.Id, newTodoItem.CreatedAtUtc));
    }
}


