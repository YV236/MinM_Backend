using MinM_API.Dtos.User;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class UserService : IUserService
    {
        public Task<ServiceResponse<GetUserDto>> GetUserInfo(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
