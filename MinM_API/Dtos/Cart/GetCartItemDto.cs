using MinM_API.Dtos.Product;
using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Cart
{
    public record GetCartItemDto { 
        public string Id {get; set;}
        string productId { get; set; }
        GetProductDto ChosenProduct { get; set; }
        string ProductVariantId { get; set; }
        GetProductVariantDto ChosenVariant { get; set; }
        int Quantity { get; set; }
        DateTime AddedAt { get; set; }
    }
}
