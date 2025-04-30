namespace MinM_API.Models
{
    public class ProductVariant
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }
    }
}
