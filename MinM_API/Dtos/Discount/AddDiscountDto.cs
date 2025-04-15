using MinM_API.Dtos.Products;
using MinM_API.Models;

namespace MinM_API.Dtos.Discount
{
    public class AddDiscountDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<string> ProductIds { get; set; } = [];
    }
}
