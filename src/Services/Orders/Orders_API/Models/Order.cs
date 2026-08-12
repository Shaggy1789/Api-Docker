namespace Orders_API.Models;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string IdempotencyKey { get; set; } = default!;
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}