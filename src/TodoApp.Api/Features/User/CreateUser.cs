using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Auth;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class CreateUser
{
    public sealed record Request(string name, string email, string password);
    public sealed record Response(Guid IdUser, DateTime CreatedAt);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.name)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .Length(2, 120);

            RuleFor(x => x.email)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .MaximumLength(180)
                .EmailAddress();

            RuleFor(x => x.password)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informada.")
                .MinimumLength(8)
                .MaximumLength(100);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(UserRoutes.Base, Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("CreateUser")
                .WithSummary("Cria um usuario")
                .WithDescription("Cria um usuario ativo com preferencias padrao.")
                .Accepts<Request>("application/json")
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    internal static async Task<IResult> Handler(
        Request request,
        AppDbContext context,
        IValidator<Request> validator,
        IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        string email = request.email.Trim();
        List<string> registeredEmails = await context.Users
            .AsNoTracking()
            .Select(user => user.Email)
            .ToListAsync();

        bool emailAlreadyExists = registeredEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

        if (emailAlreadyExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "E-mail ja cadastrado",
                detail: UserErrors.EmailAlreadyExists);
        }

        var user = new Domain.Entities.User(request.name.Trim(), email, passwordHasher.Hash(request.password));

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return Results.Created($"{UserRoutes.Base}/{user.Id}", new Response(user.Id, user.CreatedAtUtc));
    }
}
