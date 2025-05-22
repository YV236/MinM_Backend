namespace MinM_API.Dtos.User
{
    public record UserRegisterDto(
        string UserFirstName,
        string UserLastName,
        string Email,
        AddressDto AddressDto,
        string Password,
        string PhoneNumber);
}
