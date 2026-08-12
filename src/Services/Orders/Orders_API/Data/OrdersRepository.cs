using MongoDB.Driver;
using Orders_API.Models;

namespace Orders_API.Data;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Order> _ordersCollection;

    public OrdersRepository(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDb:DatabaseName"];
        if (string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("MongoDb:DatabaseName is not configured.");
        }

        _database = mongoClient.GetDatabase(databaseName);
        _ordersCollection = _database.GetCollection<Order>("Orders");
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.IdempotencyKey, idempotencyKey);
        return await _ordersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.Id, orderId);
        return await _ordersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.CustomerId, customerId);
        return await _ordersCollection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.IdempotencyKey, order.IdempotencyKey);
        var options = new ReplaceOptions { IsUpsert = true };
        await _ordersCollection.ReplaceOneAsync(filter, order, options, cancellationToken);
    }
}