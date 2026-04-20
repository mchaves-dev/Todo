namespace TodoApp.Api.Features.Users.SharedUser;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
