using MinM_API.Dtos.Product;
using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Cart
{
    public record GetCartItemDto 
    { 
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
