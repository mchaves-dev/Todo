using FluentValidation;
using TodoApp.Api.Aplication.Auth;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Auth.SharedAuth;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Auth;

public static class RefreshToken
{
    public sealed record Request(string refreshToken);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.refreshToken)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("deve ser informado.");
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost($"{AuthRoutes.Base}/refresh", Handler)
                .WithTags(AuthRoutes.Tag)
                .WithName("RefreshToken")
                .WithSummary("Renova tokens")
                .WithDescription("Valida o refresh token ativo, revoga o token usado e retorna um novo par de tokens.")
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
        ITokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(tokenService);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        AuthTokens? tokens = await tokenService.RefreshAsync(request.refreshToken.Trim(), context);

        if (tokens is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Refresh token invalido",
                detail: "Refresh token invalido, expirado ou revogado.");
        }

        await context.SaveChangesAsync();

        return Results.Ok(tokens);
    }
}
