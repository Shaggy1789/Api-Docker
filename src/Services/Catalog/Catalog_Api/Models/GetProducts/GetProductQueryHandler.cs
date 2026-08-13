using Catalog_Api.Common.Caching;
using Catalog_Api.Common.Pagination;


namespace Catalog_Api.Models.GetProducts
{
    public record GetProductsQuery(
        int PageNumber = 1,
        int PageSize = 10
        ) : IQuery<GetProductsResult>, ICacheableQuery
    {
        public string CacheKey => $"products-page-{PageNumber}-size-{PageSize}";
        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    }

    public record GetProductsResult(PaginatedResult<Product> Products);
    public class GetProductsQueryHandler(IDocumentSession session, ILogger<GetProductsQueryHandler> logger)
        : IQueryHandler<GetProductsQuery, GetProductsResult>
    {
        public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductsQueryHandler.Handle llamado con {@Query}", query);
            var totalCount = await session.Query<Product>().LongCountAsync(cancellationToken);
            //deacuerdo con la paginacion traemos los productos
            var products = await session.Query<Product>()
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);
            var paginateResult = new PaginatedResult<Product>
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Data = products

            };
            return new GetProductsResult(paginateResult);
        }

    }
}
