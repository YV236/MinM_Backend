namespace MinM_API.Models
{
    /// <summary>
    /// Клас для опису товару в базі даних
    /// </summary>
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? DiscountId { get; set; }
        public virtual Discount? Discount { get; set; }
        public decimal? DiscountPrice { get; set; } // Якщо є знижка
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public string SKU { get; set; } = string.Empty; // Артикул товару
        public virtual List<string> ImageUrls { get; set; } = new(); // Фото товару

        public virtual List<User> UsersWithThisProductInCart { get; set; } = new();
    }
}
