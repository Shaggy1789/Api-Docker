using Carter;
using Marten;

namespace Catalog_Api.Models.CreateProduct
{
    public record CreateProductRequest(string Name, string Description, List<String> Category, string ImageFile, decimal Price);
    public record CreateProductResponse(Guid Id);
    public class CreateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products",async (CreateProductRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateProductCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateProductResponse>();
                return Results.Created($"/products/{response.Id}", response);
            })
                .WithName("CrearProducto")
                .Produces<CreateProductResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Crear un nuevo Producto")
                .WithDescription("Crea a nuevo producto y se retorna el identificador de la entidad");

        }
    }
}
