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
    public class UserService(IUserRepository userRepository,
        UserManager<User> userManager, DataContext context) : IUserService
    {
        public async Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {
            var serviceResponse = new ServiceResponse<int>();

            if (!userRepository.AreAllFieldsFilled(userRegisterDto))
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = "Error while registering. Some of the properties may be filled incorrectly";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;

                return serviceResponse;
            }

            if (!userRepository.IsValidEmail(userRegisterDto.Email))
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

        public async Task<ServiceResponse<GetUserDto>> GetUserInfo(ClaimsPrincipal user)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();

            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser == null)
                {
                    serviceResponse.Data = null;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "Unable to find the user.";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;

                    return serviceResponse;
                }

                var userDto = new GetUserDto()
                {
                    UserName = getUser.UserName!,
                    UserFirstName = getUser.UserFirstName,
                    UserLastName = getUser.UserLastName,
                    Email = getUser.Email,
                    Address = new Dtos.Address()
                    {
                        Street = getUser.Address!.Street,
                        HomeNumber = getUser.Address!.HomeNumber,
                        City = getUser.Address!.City,
                        Region = getUser.Address!.Region,
                        PostalCode = getUser.Address!.PostalCode,
                        Country = getUser.Address!.Country,
                    },
                    PhoneNumber = getUser.PhoneNumber,
                };
                serviceResponse.Data = userDto;
                serviceResponse.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = null;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();
            var getUser = await userRepository.FindUser(user, context);

            if (getUser == null)
            {
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = "User not found.";
                serviceResponse.StatusCode = HttpStatusCode.NotFound;
                return serviceResponse;
            }

            if (!userRepository.AreAllFieldsFilled(userUpdateDto))
            {
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = "Error while updating. Some of the properties may be filled incorrectly.";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;
                return serviceResponse;
            }

            if (userUpdateDto.PhoneNumber.Any(c => !char.IsDigit(c)) || userUpdateDto.PhoneNumber.Length < 9)
            {
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = $"Error while registering. " +
                    $"Phone number '{userUpdateDto.PhoneNumber}' must contain numbers only and have at least 9 digits.";
                serviceResponse.StatusCode = HttpStatusCode.UnprocessableEntity;
                return serviceResponse;
            }

            getUser.UserFirstName = userUpdateDto.UserFirstName;
            getUser.UserLastName = userUpdateDto.UserLastName;
            getUser.PhoneNumber = userUpdateDto.PhoneNumber;

            getUser.Address ??= new Models.Address();

            getUser.Address.Street = userUpdateDto.Street;
            getUser.Address.HomeNumber = userUpdateDto.HomeNumber;
            getUser.Address.City = userUpdateDto.City;
            getUser.Address.Region = userUpdateDto.Region;
            getUser.Address.PostalCode = userUpdateDto.PostalCode;
            getUser.Address.Country = userUpdateDto.Country;

            context.Users.Update(getUser);

            await context.SaveChangesAsync();

            serviceResponse.Data = new GetUserDto()
            {
                UserFirstName = getUser.UserFirstName,
                UserLastName = getUser.UserLastName,
                Email = getUser.Email,
                Address = new Dtos.Address()
                {
                    Street = getUser.Address!.Street,
                    HomeNumber = getUser.Address!.HomeNumber,
                    City = getUser.Address!.City,
                    Region = getUser.Address!.Region,
                    PostalCode = getUser.Address!.PostalCode,
                    Country = getUser.Address!.Country,
                },
                PhoneNumber = getUser.PhoneNumber,
            };
            serviceResponse.IsSuccessful = true;
            serviceResponse.Message = "The data successfully updated";
            return serviceResponse;
        }
    }
}
