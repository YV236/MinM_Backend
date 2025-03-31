namespace MinM_API.Dtos.Products
{
    public class UpdateProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public List<string> ProductVariant { get; set; } = [];
        public int UnitsInStock { get; set; }
        public bool IsStock => UnitsInStock > 0;
        public string CategoryId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
    }
}
