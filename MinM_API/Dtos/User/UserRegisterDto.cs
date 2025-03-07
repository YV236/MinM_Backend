namespace MinM_API.Dtos.User
{
    public record UserRegisterDto(
        string UserFirstName,
        string UserLastName,
        string Email,
        string Street,
        string HomeNumber,
        string City,
        string? Region,
        string? PostalCode,
        string Country,
        string Password,
        string PhoneNumber);
}
