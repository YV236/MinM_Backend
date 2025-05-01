using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Products
{
    public class AddProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AddProductVariantDto> ProductVariants { get; set; } = [];
        public string CategoryId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
    }
}
