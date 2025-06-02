using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Products
{
    public class GetProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<GetProductVariantDto> ProductVariants { get; set; } = [];
        public string DiscountId { get; set; } = string.Empty;
        public bool? IsSeasonal { get; set; }
        public bool? IsDiscounted { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty; // Product article
        public virtual List<GetProductImageDto> ProductImages { get; set; } = []; // Product photo
    }
}
