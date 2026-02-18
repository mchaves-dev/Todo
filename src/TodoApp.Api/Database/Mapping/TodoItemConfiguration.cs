using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Api.Entities;

namespace TodoApp.Api.Database.Mapping;

internal sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
        entity.Property(x => x.DueDate);
        entity.Property(x => x.Labels);
        entity.Property(x => x.IsCompleted).IsRequired();
        entity.Property(x => x.CompletedAt);
        entity.Property(x => x.Priority).HasConversion<short>();
        entity.Property(x => x.IsArchived).IsRequired();
        entity.Property(x => x.ArchivedAt);
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc);
    }
}
