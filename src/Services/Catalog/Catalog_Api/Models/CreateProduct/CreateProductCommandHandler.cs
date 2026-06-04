using Marten;

namespace Catalog_Api.Models.CreateProduct
{
    /*record nos permite crear el producto con los datos para registrar como uno nuevo*/
    public record CreateProductCommand(string Name, string Description, List<string> Category, string ImageFile, decimal Price) 
        : ICommand<CreateProductResult>;
    
    /* este record retorna el objeto de respuesta es decie el identificador del objeto insertado */
    public record CreateProductResult(Guid Id);
    /*inyectamos el documento de postgre y se trabaja con IDocumentSession*/

    internal class CreateProductCommandHandler(IDocumentSession documentSession): ICommandHandler<CreateProductCommand, CreateProductResult>
    {
        public async Task<CreateProductResult> Handle (CreateProductCommand request,CancellationToken cancellationToken)
        { 
            //aqui va la logica para crear el producto y retornar el resultado
            Product product = new Product
            {
                Name =  request.Name,
                Description = request.Description,
                Category = request.Category,
                ImageFile = request.ImageFile,
                Price = request. Price

        };

            documentSession.Store(product);
            await documentSession.SaveChangesAsync(cancellationToken);
            return new CreateProductResult(product.Id);
        }
    }

}
