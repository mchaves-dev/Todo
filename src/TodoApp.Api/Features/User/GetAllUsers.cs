using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Aplication.Extensions;
using TodoApp.Api.Features.Users.SharedUser;
using TodoApp.Api.Infra.Database;

namespace TodoApp.Api.Features.Users;

public static class GetAllUsers
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(UserRoutes.Base, Handler)
                .WithTags(UserRoutes.Tag)
                .WithName("GetUsers")
                .WithSummary("Lista usuarios")
                .WithDescription("Retorna uma pagina de usuarios cadastrados.")
                .Produces<List<UserDto>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<IResult> Handler(AppDbContext context, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        ArgumentNullException.ThrowIfNull(context);

        IDictionary<string, string[]>? paginationErrors = PaginationExtensions.ValidatePagination(page, pageSize);

        if (paginationErrors is not null)
        {
            return Results.ValidationProblem(paginationErrors);
        }

        List<UserDto> users = await context
            .Users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ObterDetalhes()
            .ToListAsync();

        return Results.Ok(users);
    }
}
