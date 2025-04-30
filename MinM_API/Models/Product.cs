using Microsoft.EntityFrameworkCore;

namespace MinM_API.Models
{
    /// <summary>
    /// Class for describing the product in the database
    /// </summary>
    [Index(nameof(Slug), IsUnique = true)]
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public virtual List<ProductVariant> ProductVariant { get; set; } = [];
        public bool? IsSeasonal { get; set; } = false;
        public string? SeasonId { get; set; }
        public virtual Season? Season { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public string SKU { get; set; } = string.Empty; // Product article
        public virtual List<ProductImage> ProductImages { get; set; } = []; // Product photo

        public virtual List<User> UsersWithThisProductInCart { get; set; } = [];
    }
}
