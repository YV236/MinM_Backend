namespace MinM_API.Dtos.User
{
    public record AddressDto(
        string Street,
        string HomeNumber,
        string City,
        string Region,
        string PostalCode,
        string Country);
}
