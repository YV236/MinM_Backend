﻿using MinM_API.Dtos;
using MinM_API.Dtos.User;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<GetUserDto>> GetUserInfo(ClaimsPrincipal user);
        Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto);
        Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto);
    }
}
