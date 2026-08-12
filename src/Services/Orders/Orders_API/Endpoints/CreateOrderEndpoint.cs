using Orders_API.Application.Orders.CreateOrder;
using Carter;
using Mapster;
using MediatR;

namespace Orders_API.Endpoints;

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderCommand request, ISender sender) =>
        {
            var result = await sender.Send(request);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        })
            .WithName("CreateOrder")
            .Produces<CreateOrderResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Crear orden")
            .WithDescription("Crea una nueva orden a partir de un carrito");
    }
}