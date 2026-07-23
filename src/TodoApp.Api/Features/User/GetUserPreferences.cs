using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class GetUserPreferences
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet($"{UserRoutes.Base}/{{id:guid}}/preferences", Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("GetUserPreferences")
                .WithSummary("Consulta preferencias do usuario")
                .WithDescription("Retorna as preferencias pessoais de um usuario.")
                .RequireAuthorization()
                .Produces<UserPreferenceDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<IResult> Handler(Guid id, AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        bool userExists = await context.Users.AnyAsync(user => user.Id == id);

        if (!userExists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario nao encontrado",
                detail: UserErrors.NotFound);
        }

        UserPreferenceDto? preference = await context
            .UserPreferences
            .ObterDetalhes()
            .SingleOrDefaultAsync(x => x.UserId == id);

        return preference is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Preferencias nao encontradas",
                detail: "Preferencias do usuario nao encontradas.")
            : Results.Ok(preference);
    }
}
