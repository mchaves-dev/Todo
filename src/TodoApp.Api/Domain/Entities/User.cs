namespace TodoApp.Api.Domain.Entities;

public sealed class User : IAuditableEntity
{
    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
        Preference = null!;
    }

    public User(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        IsActive = true;
        Preference = UserPreference.CreateDefault(Id);
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public UserPreference Preference { get; set; }
    public ICollection<TodoItem> Todos { get; } = [];

    public void Update(string? name, string? email, bool? isActive)
    {
        if (name is not null)
        {
            Name = name;
        }

        if (email is not null)
        {
            Email = email;
        }

        if (isActive.HasValue)
        {
            IsActive = isActive.Value;
        }
    }
}
