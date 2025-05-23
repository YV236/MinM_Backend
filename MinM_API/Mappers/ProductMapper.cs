using MinM_API.Dtos.Products;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class ProductMapper
    {
        public partial GetProductDto ProductToGetProductDto(Product product);
    }
}
