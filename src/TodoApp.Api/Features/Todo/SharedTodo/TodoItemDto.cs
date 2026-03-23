using TodoApp.Api.Domain.Enums;

namespace TodoApp.Api.Features.Todo.SharedTodo;

public sealed record TodoItemDto(Guid Id,
        Guid UserId,
        string Description,
        DateTime? DueDate,
        string[] Labels,
        bool IsCompleted,
        DateTime? CompletedAt,
        EPriority Priority,
        bool IsArchived,
        DateTime? ArchivedAt,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
