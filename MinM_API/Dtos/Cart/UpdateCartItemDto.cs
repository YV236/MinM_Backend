namespace MinM_API.Dtos.Cart
{
    public record UpdateCartItemDto
    {
        public string Id { get; set; }
        public string ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
