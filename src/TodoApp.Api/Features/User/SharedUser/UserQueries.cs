using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Domain.Entities;

namespace TodoApp.Api.Features.Users.SharedUser;

public static class UserQueries
{
    public static IQueryable<UserDto> ObterDetalhes(this IQueryable<Domain.Entities.User> users)
    {
        return users
            .AsNoTracking()
            .Select(user => new UserDto(
                user.Id,
                user.Name,
                user.Email,
                user.IsActive,
                user.CreatedAtUtc,
                user.UpdatedAtUtc));
    }

    public static IQueryable<UserPreferenceDto> ObterDetalhes(this IQueryable<UserPreference> preferences)
    {
        return preferences
            .AsNoTracking()
            .Select(preference => new UserPreferenceDto(
                preference.Id,
                preference.UserId,
                preference.Theme,
                preference.Language,
                preference.Timezone,
                preference.DefaultPageSize,
                preference.ShowArchived,
                preference.CreatedAtUtc,
                preference.UpdatedAtUtc));
    }
}
