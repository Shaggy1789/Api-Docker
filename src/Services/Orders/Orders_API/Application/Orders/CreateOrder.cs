using BuildingBlocks.CQRS;

namespace Orders_API.Application.Orders.CreateOrder;

public record CreateOrderCommand(string CustomerId, string BasketId, string IdempotencyKey) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Guid OrderId);