using FluentValidation;
using TodoApp.Api.Database;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Entities;
using TodoApp.Api.Enums;
using TodoApp.Api.Extensions;

namespace TodoApp.Api.Features.Todo;

public static class CreateTodo
{
    public sealed record Request(Guid userId, string description, EPriority priority, DateTime? dueDate = null, string [] labels = null);
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
            app.MapPost("todoitem", Handler)
                .WithTags("Todo Item");
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
            return Results.BadRequest(validationResult.ToFormattedErrorMessages());
        }

        var todoItem = new TodoItem(request.userId, request.description, request.priority, request.dueDate, request.labels);

        await context.AddAsync(todoItem);
        await context.SaveChangesAsync();

        return Results.Ok(new Response(todoItem.Id, todoItem.CreatedAtUtc));
    }
}

