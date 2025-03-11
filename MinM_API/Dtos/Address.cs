namespace MinM_API.Dtos
{
    public record Address
    {
        public string? Street { get; set; } = string.Empty;
        public string? HomeNumber { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public string? Region { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
    }
}
