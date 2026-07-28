namespace Catalog_Api.Models.GetProductByCategory
{
    //public record GetProductByCategoryRequest()
    public record GetProductByCategoryResponse(IEnumerable<Product> Products);
    public class GetproductByCategoryEndPoint: ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/category/{ category}", async (string category, ISender sender)=>
            {
                var result = await sender.Send(new GetProductByCategoryQuery(category));
                var response = result.Adapt<GetProductByCategoryResponse>();
                return Results.Ok(response);
            })
                .WithName("GetProductByCategory")
                .Produces<GetProductByCategoryResponse>(StatusCodes.Status200OK) 
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("obtener productos por categoria")
                .WithDescription("obtener productos por Categoria");
        }
    }
}
