namespace MinM_API.Dtos.Products
{
    public class AddProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProductVariantDto> ProductVariant { get; set; } = [];
        public string CategoryId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
    }
}
