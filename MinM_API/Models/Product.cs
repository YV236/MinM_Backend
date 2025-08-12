using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTime DateOfCreation { get; set; }
        public bool IsNew { get; set; }
        public virtual List<ProductVariant> ProductVariants { get; set; } = [];
        public string? DiscountId { get; set; }
        public virtual Discount? Discount { get; set; }
        public bool IsDiscounted { get; set; } = false;
        public bool? IsSeasonal { get; set; } = false;
        public string? SeasonId { get; set; }
        public virtual Season? Season { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public string SKU { get; set; } = string.Empty; // Product article
        public virtual List<ProductImage> ProductImages { get; set; } = []; // Product photo
        public virtual List<Color>? Colors { get; set; } = [];
        public char SKUGroup { get; set; }
        public int SKUSequence { get; set; }
    }
}
