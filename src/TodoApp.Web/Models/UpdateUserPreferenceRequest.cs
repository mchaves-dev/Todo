namespace TodoApp.Web.Models;

public sealed record UpdateUserPreferenceRequest(
    string? Theme,
    string? Language,
    string? Timezone,
    int? DefaultPageSize,
    bool? ShowArchived);

