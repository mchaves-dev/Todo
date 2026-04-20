using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Aplication.Extensions;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Domain.Enums;
using TodoApp.Api.Features.Todo.SharedTodo;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Todo;

public static class CreateTodo
{
    public sealed record Request(Guid userId, string description, EPriority priority, DateTime? dueDate = null, string[]? labels = null);
    public sealed record Response(Guid IdTodoItem, DateTime CreatedAt);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.userId).NotEmpty();

            RuleFor(x => x.description)
                .NotEmpty()
                .Length(2, 200);

            RuleFor(x => x.priority).IsInEnum();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(TodoRoutes.Base, Handler)
                .WithTags(TodoRoutes.Tag)
                .WithName("CreateTodoItem")
                .WithSummary("Cria um item de tarefa")
                .WithDescription("""
                    Cria um novo item de tarefa para um usuario.
                    A descricao deve ter entre 2 e 200 caracteres e a prioridade deve ser um valor valido do enum EPriority.
                    """)
                .Accepts<Request>("application/json")
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    internal static async Task<IResult> Handler(Request request,
        AppDbContext context,
        IValidator<Request> validator)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.userId);

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

        var todoItem = new TodoItem(request.userId, request.description, request.priority, request.dueDate, request.labels);

        await context.AddAsync(todoItem);
        await context.SaveChangesAsync();

        return Results.Created($"{TodoRoutes.Base}/{todoItem.Id}", new Response(todoItem.Id, todoItem.CreatedAtUtc));
    }
}

