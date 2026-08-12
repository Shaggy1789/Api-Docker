using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Orders_API.Data;
using Orders_API.Models;

namespace Orders_API.Application.Orders.GetOrder;

public record GetOrderQuery(Guid OrderId) : IQuery<GetOrderResult>;

public record GetOrderResult(Order Order);

public class GetOrderHandler(IOrdersRepository repository) : IQueryHandler<GetOrderQuery, GetOrderResult>
{
    private readonly IOrdersRepository _repository = repository;

    public async Task<GetOrderResult> Handle(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Orden no encontrada");
        }

        return new GetOrderResult(order);
    }
}