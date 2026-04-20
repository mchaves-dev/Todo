using TodoApp.Api.Domain.Enums;

namespace TodoApp.Api.Domain.Entities;

public sealed class TodoItem : IAuditableEntity
{
    public TodoItem(Guid userId, string description, EPriority priority, DateTime? dueDate = null, string[] labels = null)
    {
        Id = Guid.NewGuid();
        IsCompleted = false;
        IsArchived = false;
        UpdatedAtUtc = null;

        UserId = userId;
        Description = description;
        DueDate = dueDate;
        Labels = labels;
        Priority = priority;
    }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string[] Labels { get; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    internal EPriority Priority { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public void Complete()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void Archived()
    {
        if (IsArchived)
            return;

        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
    }

    public TodoItem Clone()
    {
        return new TodoItem(UserId, Description, Priority, DueDate, Labels);
    }
}
