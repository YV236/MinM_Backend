﻿namespace MinM_API.Dtos.User
{
    public record UpdateUserDto(
        string UserFirstName,
        string UserLastName,
        string PhoneNumber,
        AddressDto AddressDto);
}
