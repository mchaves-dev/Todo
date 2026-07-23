namespace TodoApp.Web.Models;

public sealed record CreateUserRequest(string Name, string Email, string Password);

