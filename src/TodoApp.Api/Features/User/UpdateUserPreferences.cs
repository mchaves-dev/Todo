using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Domain.Entities;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class UpdateUserPreferences
{
    public sealed record Request(string? theme, string? language, string? timezone, int? defaultPageSize, bool? showArchived);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.theme)
                .Must(theme => theme is not null && UserPreference.AcceptedThemes.Contains(theme, StringComparer.Ordinal))
                .WithMessage("deve ser System, Light ou Dark.")
                .When(x => x.theme is not null);

            RuleFor(x => x.language)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .MaximumLength(10)
                .When(x => x.language is not null);

            RuleFor(x => x.timezone)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.")
                .MaximumLength(80)
                .When(x => x.timezone is not null);

            RuleFor(x => x.defaultPageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .When(x => x.defaultPageSize.HasValue);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch($"{UserRoutes.Base}/{{id:guid}}/preferences", Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("UpdateUserPreferences")
                .WithSummary("Atualiza preferencias do usuario")
                .WithDescription("Atualiza parcialmente as preferencias pessoais de um usuario.")
                .Accepts<Request>("application/json")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound)
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

        bool userExists = await context.Users.AnyAsync(user => user.Id == id);

        if (!userExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario nao encontrado",
                detail: UserErrors.NotFound);
        }

        UserPreference? preference = await context.UserPreferences.SingleOrDefaultAsync(x => x.UserId == id);

        if (preference is null)
        {
            preference = UserPreference.CreateDefault(id);
            await context.UserPreferences.AddAsync(preference);
        }

        preference.Update(
            request.theme,
            request.language?.Trim(),
            request.timezone?.Trim(),
            request.defaultPageSize,
            request.showArchived);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
