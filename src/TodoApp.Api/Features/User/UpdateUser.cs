using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class UpdateUser
{
    public sealed record Request(string? name, string? email, bool? isActive);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.name)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .Length(2, 120)
                .When(x => x.name is not null);

            RuleFor(x => x.email)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .MaximumLength(180)
                .EmailAddress()
                .When(x => x.email is not null);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch($"{UserRoutes.Base}/{{id:guid}}", Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("UpdateUser")
                .WithSummary("Atualiza um usuario")
                .WithDescription("Atualiza nome, e-mail e status ativo de um usuario.")
                .Accepts<Request>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    internal static async Task<IResult> Handler(
        Guid id,
        Request request,
        AppDbContext context,
        IValidator<Request> validator)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(validator);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        Domain.Entities.User? user = await context.Users.FindAsync(id);

        if (user is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario nao encontrado",
                detail: UserErrors.NotFound);
        }

        string? email = request.email?.Trim();

        if (email is not null)
        {
            List<string> registeredEmails = await context.Users
                .AsNoTracking()
                .Where(existingUser => existingUser.Id != id)
                .Select(existingUser => existingUser.Email)
                .ToListAsync();

            bool emailAlreadyExists = registeredEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

            if (emailAlreadyExists)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "E-mail ja cadastrado",
                    detail: UserErrors.EmailAlreadyExists);
            }
        }

        user.Update(request.name?.Trim(), email, request.isActive);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
