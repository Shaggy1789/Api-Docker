using Carter;
using Mapster;
using MediatR;
using Orders_API.Application.Orders.GetOrder;
using Orders_API.Models;

namespace Orders_API.Endpoints;

public class GetOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderQuery(id));
            return Results.Ok(result);
        })
            .WithName("GetOrder")
            .Produces<GetOrderResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Obtener orden")
            .WithDescription("Obtiene una orden por su identificador");
    }
}