namespace TodoApp.Web.Models;

public sealed record TodoItem(
    Guid Id,
    Guid UserId,
    string Description,
    DateTime? DueDate,
    string[] Labels,
    bool IsCompleted,
    DateTime? CompletedAt,
    TodoPriority Priority,
    bool IsArchived,
    DateTime? ArchivedAt,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

