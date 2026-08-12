using BuildingBlocks.CQRS;
using Orders_API.Data;
using Orders_API.Models;

namespace Orders_API.Application.Orders.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(string CustomerId) : IQuery<GetOrdersByCustomerResult>;

public record GetOrdersByCustomerResult(List<Order> Orders);

public class GetOrdersByCustomerHandler(IOrdersRepository repository) : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    private readonly IOrdersRepository _repository = repository;

    public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        return new GetOrdersByCustomerResult(orders);
    }
}