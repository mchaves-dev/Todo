using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Api.Domain.Entities;

namespace TodoApp.Api.Infra.Database.Mapping;

internal sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.HasKey(x => x.Id);
        entity.Property(x => x.UserId).IsRequired();
        entity.Property(x => x.Theme).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Language).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Timezone).HasMaxLength(80).IsRequired();
        entity.Property(x => x.DefaultPageSize).IsRequired();
        entity.Property(x => x.ShowArchived).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc);

        entity.HasIndex(x => x.UserId).IsUnique();
    }
}
