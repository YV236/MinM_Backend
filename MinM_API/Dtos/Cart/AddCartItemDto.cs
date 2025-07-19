namespace MinM_API.Dtos.Cart
{
    public record AddCartItemDto 
    {
        public string ProductId { get; set; }
        public string ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
