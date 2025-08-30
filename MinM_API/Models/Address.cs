namespace MinM_API.Models
{
    public abstract class Address
    {
        public string Id { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}
