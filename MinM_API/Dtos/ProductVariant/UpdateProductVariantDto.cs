namespace MinM_API.Dtos.ProductVariant
{
    public class UpdateProductVariantDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public bool IsStock => UnitsInStock > 0;
    }
}
