namespace MinM_API.Dtos.Products
{
    public class GetProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public List<string> ProductVariant { get; set; } = [];
        public decimal? DiscountPrice { get; set; } // If there is a discount
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }
        public bool? IsSeasonal { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty; // Product article
        public virtual List<GetProductImageDto> ImageUrls { get; set; } = []; // Product photo
    }
}
