namespace TodoApp.Web.Models;

public sealed record CreateTodoRequest(
    Guid UserId,
    string Description,
    TodoPriority Priority,
    DateTime? DueDate,
    string[]? Labels);

