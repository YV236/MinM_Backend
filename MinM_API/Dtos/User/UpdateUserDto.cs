namespace MinM_API.Dtos.User
{
    public record UpdateUserDto(
        string UserFirstName,
        string UserLastName,
        string Street,
        string HomeNumber,
        string City,
        string Region,
        string PostalCode,
        string Country,
        string PhoneNumber);
}
