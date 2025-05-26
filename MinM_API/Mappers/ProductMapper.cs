using MinM_API.Dtos.Products;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class ProductMapper
    {
        public partial GetProductDto ProductToGetProductDto(Product product);

        [MapperIgnoreSource(nameof(Product.Id))]
        [MapperIgnoreSource(nameof(Product.Slug))]
        [MapperIgnoreSource(nameof(Product.ProductVariants))]
        [MapperIgnoreSource(nameof(Product.ProductImages))]
        [MapperIgnoreSource(nameof(Product.Season))]
        [MapperIgnoreSource(nameof(Product.SeasonId))]
        [MapperIgnoreSource(nameof(Product.Discount))]
        [MapperIgnoreSource(nameof(Product.DiscountId))]
        public partial void UpdateProductToProduct(UpdateProductDto productDto, Product product);
    }
}
