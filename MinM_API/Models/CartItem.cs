namespace MinM_API.Models
{
    public class CartItem
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
