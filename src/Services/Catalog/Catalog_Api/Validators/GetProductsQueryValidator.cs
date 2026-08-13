using Catalog_Api.Models.GetProducts;

namespace Catalog_Api.Validators
{
public class GetProductsQueryValidator: AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El numero de paginas debe ser mayor o igual a 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de la pagina debe estar entre 1 y 100.");
    }
}
}
