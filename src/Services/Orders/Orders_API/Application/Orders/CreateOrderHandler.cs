using Carter;
using Mapster;
using MediatR;
using Orders_API.Data;
using Orders_API.Models;
using BuildingBlocks.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orders_API.Application.Orders.CreateOrder;

public class CreateOrderHandler(IOrdersRepository repository, IHttpClientFactory httpClientFactory)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrdersRepository _repository = repository;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // 1. Validate Idempotency-Key - check if order already exists
        var existingOrder = await _repository.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        if (existingOrder is not null)
        {
            return new CreateOrderResult(existingOrder.Id);
        }

        // 2. Consult Basket API
        var basketClient = _httpClientFactory.CreateClient("BasketApi");
        var basketResponse = await basketClient.GetAsync($"/basket/{command.BasketId}", cancellationToken);

        if (!basketResponse.IsSuccessStatusCode)
        {
            throw new BadRequestException("No se pudo consultar el carrito");
        }

        var basketContent = await basketResponse.Content.ReadAsStringAsync(cancellationToken);
        JsonDocument basketDoc;
        try
        {
            basketDoc = JsonDocument.Parse(basketContent);
        }
        catch
        {
            throw new BadRequestException("No se pudo parsear el carrito");
        }

        // 3. Verify basket exists and not empty
        var cart = basketDoc.RootElement.TryGetProperty("cart", out var cartElement)
            ? cartElement
            : basketDoc.RootElement;

        if (!cart.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
        {
            throw new BadRequestException("El carrito está vacío");
        }

        // 4. Validate products and quantities, get prices
        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;

        foreach (var item in items.EnumerateArray())
        {
            var unitPrice = item.TryGetProperty("price", out var priceElement) ? priceElement.GetDecimal() : 0;
            var quantity = item.TryGetProperty("quantity", out var quantityElement) ? quantityElement.GetInt32() : 1;

            orderItems.Add(new OrderItem
            {
                ProductId = item.TryGetProperty("productId", out var productIdElement) ? Guid.Parse(productIdElement.GetString()) : Guid.Empty,
                ProductName = item.TryGetProperty("productName", out var productNameElement) ? productNameElement.GetString() : "Producto",
                Quantity = quantity,
                UnitPrice = unitPrice,
                LineTotal = unitPrice * quantity
            });

            subtotal += unitPrice * quantity;
        }

        // 5. Calculate taxes (e.g., 16% VAT)
        decimal tax = subtotal * 0.16m;
        decimal total = subtotal + tax;

        // 6. Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = orderItems,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            IdempotencyKey = command.IdempotencyKey
        };

        await _repository.SaveAsync(order, cancellationToken);

        return new CreateOrderResult(order.Id);
    }
}