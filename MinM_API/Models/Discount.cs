using Microsoft.EntityFrameworkCore;

namespace MinM_API.Models
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Discount
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool RemoveAfterExpiration { get; set; } = false;

        public virtual List<Product> Products { get; set; } = [];
    }
}
