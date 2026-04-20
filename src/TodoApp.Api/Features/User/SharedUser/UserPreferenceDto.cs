namespace TodoApp.Api.Features.Users.SharedUser;

public sealed record UserPreferenceDto(
    Guid Id,
    Guid UserId,
    string Theme,
    string Language,
    string Timezone,
    int DefaultPageSize,
    bool ShowArchived,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
