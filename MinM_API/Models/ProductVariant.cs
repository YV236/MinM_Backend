namespace MinM_API.Models
{
    public class ProductVariant
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public virtual List<Product> Product { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }

        public string? DiscountId { get; set; }
        public virtual Discount? Discount { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsDiscounted { get; set; } = false;
    }
}
