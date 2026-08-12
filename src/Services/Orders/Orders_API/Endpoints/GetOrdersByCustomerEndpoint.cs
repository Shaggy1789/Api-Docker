using Carter;
using Mapster;
using MediatR;
using Orders_API.Application.Orders.GetOrdersByCustomer;
using Orders_API.Models;

namespace Orders_API.Endpoints;

public class GetOrdersByCustomerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/customer/{customerId}", async (string customerId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));
            return Results.Ok(result);
        })
            .WithName("GetOrdersByCustomer")
            .Produces<GetOrdersByCustomerResult>(StatusCodes.Status200OK)
            .WithSummary("Obtener órdenes por cliente")
            .WithDescription("Obtiene todas las órdenes de un cliente específico");
    }
}