using Basket.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.Api.Data
{
    public class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
    {

        public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
        {
            var cachedBasket = await TryGetAsync(userName, cancellationToken);
            if (!string.IsNullOrEmpty(cachedBasket))
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);
            }
            var basket = await repository.GetBasket(userName, cancellationToken);
            await TrySetAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }

        public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            await repository.StoreBasket(basket, cancellationToken);
            await TrySetAsync(basket.Username, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            await repository.DeleteBasket(userName, cancellationToken);
            try
            {
                await cache.RemoveAsync(userName, cancellationToken);
            }
            catch
            {
                // Redis no disponible: continuar con la base de datos
            }
            return true;
        }

        private async Task<string?> TryGetAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                return await cache.GetStringAsync(key, cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        private async Task TrySetAsync(string key, string value, CancellationToken cancellationToken)
        {
            try
            {
                await cache.SetStringAsync(key, value, cancellationToken);
            }
            catch
            {
                // Redis no disponible: continuar con la base de datos
            }
        }
    }
}
