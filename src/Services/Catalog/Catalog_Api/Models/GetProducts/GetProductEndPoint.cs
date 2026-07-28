using Catalog_Api.Common.Pagination;

namespace Catalog_Api.Models.GetProducts
{
    public record GetProductsResponse(PaginatedResult<Product> Products);
    public class GetProductEndPoint: ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", 
                async (int pageNumber, int pageSize, ISender sender) =>
            {
                var query = new GetProductsQuery(pageNumber, pageSize);
                var result = await sender.Send(query);
                var response = result.Adapt<GetProductsResponse>();

                return Results.Ok(response);
            })
                .WithName("GetProducts")
                .Produces<GetProductsResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Get Resumen")
                .WithDescription("Get Resumen");
        }
    }
}
