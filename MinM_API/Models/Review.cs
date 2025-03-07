namespace MinM_API.Models
{
    public class Review
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public ushort Rating { get; set; } // Від 1 до 5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
