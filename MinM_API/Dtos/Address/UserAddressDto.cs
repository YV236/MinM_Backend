namespace MinM_API.Dtos.Address
{
    public class UserAddressDto : AddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string HomeNumber { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
    }
}
