using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Products
{
    public class UpdateProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<UpdateProductVariantDto> ProductVariants { get; set; } = [];
        public string CategoryId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
    }
}
