using Orders_API.Models;

namespace Orders_API.Data;

public interface IOrdersRepository
{
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserIdsAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}