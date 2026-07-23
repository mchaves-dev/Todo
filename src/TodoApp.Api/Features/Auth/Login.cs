using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Auth;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Auth.SharedAuth;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Auth;

public static class Login
{
    public sealed record Request(string email, string password);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.email)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .MaximumLength(180)
                .EmailAddress();

            RuleFor(x => x.password)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informada.");
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost($"{AuthRoutes.Base}/login", Handler)
                .WithTags(AuthRoutes.Tag)
                .WithName("Login")
                .WithSummary("Autentica usuario")
                .WithDescription("Valida e-mail e senha, retornando token de acesso e refresh token.")
                .AllowAnonymous()
                .Accepts<Request>("application/json")
                .Produces<AuthTokens>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    internal static async Task<IResult> Handler(
        Request request,
        AppDbContext context,
        IValidator<Request> validator,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(tokenService);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        string email = request.email.Trim();
        var users = await context.Users.ToListAsync();
        var user = users.SingleOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.password, user.PasswordHash))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciais invalidas",
                detail: "E-mail ou senha invalidos.");
        }

        AuthTokens tokens = await tokenService.IssueTokensAsync(user, context);
        await context.SaveChangesAsync();

        return Results.Ok(tokens);
    }
}
