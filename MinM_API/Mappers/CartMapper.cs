using MinM_API.Dtos.Cart;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class CartMapper
    {
        [MapperIgnoreSource(nameof(CartItem.Product.ProductVariants))]
        public partial GetCartItemDto CarItemToGetCartItemDto(CartItem cartItem);

    }
}
