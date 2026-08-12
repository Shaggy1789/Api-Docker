using Orders_API.Application.Orders.UpdateOrderStatus;
using Orders_API.Models;
using Carter;
using Mapster;
using MediatR;

namespace Orders_API.Endpoints;

public class UpdateOrderStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/orders/{id}/status", async (Guid id, OrderStatus newStatus, ISender sender) =>
        {
            var result = await sender.Send(new UpdateOrderStatusCommand(id, newStatus));
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            return Results.BadRequest(result.ErrorMessage);
        })
            .WithName("UpdateOrderStatus")
            .Produces<UpdateOrderStatusResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Actualizar estado de orden")
            .WithDescription("Actualiza el estado de una orden (Pending -> Confirmed | Pending -> Cancelled)");
    }
}