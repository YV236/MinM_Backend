namespace MinM_API.Models
{
    /// <summary>
    /// Class for describing the product in the database
    /// </summary>
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? DiscountId { get; set; }
        public virtual Discount? Discount { get; set; }
        public decimal? DiscountPrice { get; set; } // If there is a discount
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public string SKU { get; set; } = string.Empty; // Product article
        public virtual List<string> ImageUrls { get; set; } = []; // Product photo

        public virtual List<User> UsersWithThisProductInCart { get; set; } = [];
    }
}
