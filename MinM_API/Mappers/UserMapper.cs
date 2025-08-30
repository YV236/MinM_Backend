using MinM_API.Dtos.Address;
using MinM_API.Dtos.User;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class UserMapper
    {
        public partial GetUserDto UserToGetUserDto(User user);

        [MapperIgnoreSource(nameof(User.Cart))]
        [MapperIgnoreSource(nameof(User.WishList))]
        [MapperIgnoreSource(nameof(User.History))]
        [MapperIgnoreSource(nameof(User.DateOfCreation))]
        [MapperIgnoreSource(nameof(User.NormalizedEmail))]
        [MapperIgnoreSource(nameof(User.NormalizedUserName))]
        [MapperIgnoreSource(nameof(User.Email))]
        [MapperIgnoreSource(nameof(User.EmailConfirmed))]
        [MapperIgnoreSource(nameof(User.PasswordHash))]
        public partial void UpdateUserDtoToUserModel(UpdateUserDto userDto, User user);

        [MapperIgnoreSource(nameof(Address.Id))]
        public partial void UpdateAddressDtoToAddress(UserAddressDto addressDto, Address address);
    }
}
