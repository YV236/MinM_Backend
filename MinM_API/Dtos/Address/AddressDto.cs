namespace MinM_API.Dtos.Address
{
    public abstract class AddressDto
    {
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}
