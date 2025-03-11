using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.User;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using MinM_API.Services.Interfaces;
using System.Net;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class UserService(IUserRepository registrationRepository,
        UserManager<User> userManager, DataContext context) : IUserService
    {
        public async Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {
            var serviceResponse = new ServiceResponse<int>();

            if (!registrationRepository.AreAllFieldsFilled(userRegisterDto))
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = "Error while registering. Some of the properties may be filled incorrectly";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;

                return serviceResponse;
            }

            if (!registrationRepository.IsValidEmail(userRegisterDto.Email))
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = $"Registration failed." +
                    $" The email '{userRegisterDto.Email}' must include '@' and a domain such as '.com' or '.pl'.";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;

                return serviceResponse;
            }

            if (userRegisterDto.PhoneNumber.Any(c => !char.IsDigit(c)) || userRegisterDto.PhoneNumber.Length < 9)
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = $"Error while registering. Phone number" +
                    $" '{userRegisterDto.PhoneNumber}' must contain numbers only. And contain at least 9 digits";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;

                return serviceResponse;
            }

            try
            {
                var user = new User
                {
                    UserFirstName = userRegisterDto.UserFirstName,
                    UserLastName = userRegisterDto.UserLastName,
                    Email = userRegisterDto.Email,
                    UserName = userRegisterDto.Email,
                    PhoneNumber = userRegisterDto.PhoneNumber,
                    DateOfCreation = DateTime.Now,
                    Address = new Models.Address
                    {
                        Id = Guid.NewGuid().ToString(),
                        Street = userRegisterDto.Street,
                        HomeNumber = userRegisterDto.HomeNumber,
                        City = userRegisterDto.City,
                        Region = userRegisterDto.Region,
                        PostalCode = userRegisterDto.PostalCode,
                        Country = userRegisterDto.Country,
                    }
                };

                user.AddressId = user.Address.Id;

                var result = await userManager.CreateAsync(user, userRegisterDto.Password);

                if (!result.Succeeded)
                {
                    serviceResponse.Data = 0;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    serviceResponse.StatusCode = HttpStatusCode.BadRequest;

                    return serviceResponse;
                }

                serviceResponse.Data = 1;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "User registered successfully";

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }
        }

        public Task<ServiceResponse<GetUserDto>> GetUserInfo(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
