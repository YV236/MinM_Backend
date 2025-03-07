namespace MinM_API.Models
{
    public class Address
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Region { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
