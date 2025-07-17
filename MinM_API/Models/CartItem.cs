namespace MinM_API.Models
{
    public class CartItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public string ProductVariantId { get; set; }
        public virtual ProductVariant ProductVariant { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

}
