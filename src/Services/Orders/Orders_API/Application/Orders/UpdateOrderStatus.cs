using BuildingBlocks.CQRS;
using Orders_API.Data;
using Orders_API.Models;

namespace Orders_API.Application.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : ICommand<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(bool IsSuccess, string? ErrorMessage);

public class UpdateOrderStatusHandler(IOrdersRepository repository) : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IOrdersRepository _repository = repository;

    private static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            _ => false
        };
    }

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, cancellationToken);

        if (order is null)
        {
            return new UpdateOrderStatusResult(false, "Orden no encontrada");
        }

        if (!IsValidTransition(order.Status, command.NewStatus))
        {
            return new UpdateOrderStatusResult(false, "Transición de estado no permitida");
        }

        order.Status = command.NewStatus;
        await _repository.SaveAsync(order, cancellationToken);

        return new UpdateOrderStatusResult(true, null);
    }
}