using Carter;
using Mapster;
using MediatR;
using Orders_API.Application.Users;

namespace Orders_API.Endpoints;

public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (ISender sender) =>
        {
            var result = await sender.Send(new GetUsersQuery());
            return Results.Ok(result);
        })
            .WithName("GetUsers")
            .Produces<GetUsersResult>(StatusCodes.Status200OK)
            .WithSummary("Obtener usuarios")
            .WithDescription("Obtiene la lista de usuarios disponibles desde la base de datos");
    }
}