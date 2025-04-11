using MinM_API.Dtos.Products;

namespace MinM_API.Dtos.Discount
{
    public class GetDiscountDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<GetProductDto> Products { get; set; } = [];
    }
}
