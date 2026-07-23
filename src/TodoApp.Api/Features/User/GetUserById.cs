using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class GetUserById
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet($"{UserRoutes.Base}/{{id:guid}}", Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("GetUserById")
                .WithSummary("Consulta um usuario")
                .WithDescription("Retorna os dados de um usuario pelo identificador.")
                .RequireAuthorization()
                .Produces<UserDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<IResult> Handler(Guid id, AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        UserDto? user = await context.Users.ObterDetalhes().SingleOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Usuario nao encontrado",
                detail: UserErrors.NotFound);
        }

        return Results.Ok(user);
    }
}
