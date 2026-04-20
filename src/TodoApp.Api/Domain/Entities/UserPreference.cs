namespace TodoApp.Api.Domain.Entities;

public sealed class UserPreference : IAuditableEntity
{
    public static readonly string[] AcceptedThemes = ["System", "Light", "Dark"];

    public static UserPreference CreateDefault(Guid userId)
    {
        return new UserPreference(userId, "System", "pt-BR", "America/Sao_Paulo", 5, false);
    }

    private UserPreference()
    {
        Theme = string.Empty;
        Language = string.Empty;
        Timezone = string.Empty;
    }

    private UserPreference(Guid userId, string theme, string language, string timezone, int defaultPageSize, bool showArchived)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Theme = theme;
        Language = language;
        Timezone = timezone;
        DefaultPageSize = defaultPageSize;
        ShowArchived = showArchived;
    }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Theme { get; set; }
    public string Language { get; set; }
    public string Timezone { get; set; }
    public int DefaultPageSize { get; set; }
    public bool ShowArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public User User { get; set; } = null!;

    public void Update(string? theme, string? language, string? timezone, int? defaultPageSize, bool? showArchived)
    {
        if (theme is not null)
        {
            Theme = theme;
        }

        if (language is not null)
        {
            Language = language;
        }

        if (timezone is not null)
        {
            Timezone = timezone;
        }

        if (defaultPageSize.HasValue)
        {
            DefaultPageSize = defaultPageSize.Value;
        }

        if (showArchived.HasValue)
        {
            ShowArchived = showArchived.Value;
        }
    }
}
