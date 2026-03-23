using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Domain.Entities;

namespace TodoApp.Api.Features.Todo.SharedTodo;

public static class TodoItemQueries
{
    public static IQueryable<TodoItemDto> ObterDetalhes(this IQueryable<TodoItem> todos)
    {
        return todos
            .AsNoTracking()
            .Select(t =>
                new TodoItemDto(
                    t.Id,
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
                    t.UpdatedAtUtc));
    }
}
