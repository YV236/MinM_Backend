namespace MinM_API.Dtos.Discount
{
    public class UpdateDiscountDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<string> ProductIds { get; set; } = [];
    }
}
