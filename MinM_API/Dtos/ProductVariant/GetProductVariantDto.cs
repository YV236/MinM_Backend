namespace MinM_API.Dtos.ProductVariant
{
    public class GetProductVariantDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int UnitsInStock { get; set; }
        public bool IsStock { get; set; }
    }
}
