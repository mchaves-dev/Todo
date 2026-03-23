namespace TodoApp.Api.Domain.Entities;

internal interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}
