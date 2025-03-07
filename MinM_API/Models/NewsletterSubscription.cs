namespace MinM_API.Models
{
    public class NewsletterSubscription
    {
        public string Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual User? User { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
