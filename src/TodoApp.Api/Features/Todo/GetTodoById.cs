using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Database;
using TodoApp.Api.DTOs.Todo;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Entities;

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

        var todoItem = await context
                                    .Todos
                                    .Select(t =>
                                        new TodoItemDto(t.Id,
                                            t.UserId,
                                            t.Description,
                                            t.DueDate,
                                            t.Labels,
                                            t.IsCompleted,
                                            t.CompletedAt,
                                            t.Priority,
                                            t.IsArchived,
                                            t.ArchivedAt,
                                            t.CreatedAtUtc,
                                            t.UpdatedAtUtc))
                                    .SingleOrDefaultAsync(x => x.Id == id);

        if (todoItem is null)
        {
            return Results.NotFound(TodoItemError.NotFound);
        }

        return Results.Ok(todoItem);
    }
}
