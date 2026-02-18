using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Database;
using TodoApp.Api.DTOs.Todo;
using TodoApp.Api.Endpoints;

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

        var todoItens = await context
                                    .Todos
                                    .Skip(page * pageSize)
                                    .Take(pageSize)
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
                                    .AsNoTracking()
                                    .ToListAsync();

        if (todoItens.Count == 0)
        {
            return Results.NoContent();
        }

        return Results.Ok(todoItens);
    }
}
