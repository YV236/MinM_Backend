using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.User;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using MinM_API.Services.Interfaces;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MinM_API.Services.Implementations
{
    public class UserService(IUserRepository userRepository, UserManager<User> userManager, DataContext context, UserMapper mapper) : IUserService
    {
        public async Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {

            if (!AreAllFieldsFilled(userRegisterDto))
            {
                return ResponseFactory.Error(0,
                    "Error while registering. Some of the properties may be filled incorrectly",
                    HttpStatusCode.UnprocessableEntity);
            }

            if (!IsValidEmail(userRegisterDto.Email))
            {
                return ResponseFactory.Error(0,
                    $"Registration failed. The email '{userRegisterDto.Email}' must include '@' and a domain such as '.com' or '.pl'.",
                    HttpStatusCode.UnprocessableEntity);
            }

            if (userRegisterDto.PhoneNumber.Any(c => !char.IsDigit(c)) || userRegisterDto.PhoneNumber.Length < 9)
            {
                return ResponseFactory.Error(0,
                    $"Error while registering. Phone number '{userRegisterDto.PhoneNumber}' must contain numbers only. And contain at least 9 digits",
                    HttpStatusCode.UnprocessableEntity);
            }

            try
            {
                var user = new User
                {
                    Slug = SlugExtension.GenerateSlug(userRegisterDto.Email),
                    UserFirstName = userRegisterDto.UserFirstName,
                    UserLastName = userRegisterDto.UserLastName,
                    Email = userRegisterDto.Email,
                    UserName = userRegisterDto.Email,
                    PhoneNumber = userRegisterDto.PhoneNumber,
                    DateOfCreation = DateTime.Now,
                    Address = new Models.Address
                    {
                        Id = Guid.NewGuid().ToString(),
                        Street = userRegisterDto.AddressDto.Street,
                        HomeNumber = userRegisterDto.AddressDto.HomeNumber,
                        City = userRegisterDto.AddressDto.City,
                        Region = userRegisterDto.AddressDto.Region,
                        PostalCode = userRegisterDto.AddressDto.PostalCode,
                        Country = userRegisterDto.AddressDto.Country,
                    }
                };

                user.AddressId = user.Address.Id;

                var result = await userManager.CreateAsync(user, userRegisterDto.Password);

                if (!result.Succeeded)
                {
                    return ResponseFactory.Error(0, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                var addRoleResult = await userManager.AddToRoleAsync(user, "User");

                if (!addRoleResult.Succeeded)
                {
                    return ResponseFactory.Error(0, "Failed to assign role to user.");
                }

                return ResponseFactory.Success(1, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<GetUserDto>> GetUserInfo(ClaimsPrincipal user)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser == null)
                {
                    return ResponseFactory.Error(new GetUserDto(), "Unable to find the user.", HttpStatusCode.NotFound);
                }

                var userDto = mapper.UserToGetUserDto(getUser);

                return ResponseFactory.Success(userDto);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(new GetUserDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto)
        {
            var getUser = await userRepository.FindUser(user, context);

            if (getUser == null)
            {
                return ResponseFactory.Error(new GetUserDto(), "User not found.", HttpStatusCode.NotFound);
            }

            if (!AreAllFieldsFilled(userUpdateDto))
            {
                return ResponseFactory.Error(new GetUserDto(),
                    "Error while updating. Some of the properties may be filled incorrectly.",
                    HttpStatusCode.UnprocessableEntity);
            }

            if (userUpdateDto.PhoneNumber.Any(c => !char.IsDigit(c)) || userUpdateDto.PhoneNumber.Length < 9)
            {
                return ResponseFactory.Error(new GetUserDto(),
                    $"Error while registering. Phone number '{userUpdateDto.PhoneNumber}' must contain numbers only and have at least 9 digits.",
                    HttpStatusCode.UnprocessableEntity);
            }

            mapper.UpdateUserDtoToUserModel(userUpdateDto, getUser);
            mapper.UpdateAddressDtoToAddress(userUpdateDto.AddressDto, getUser.Address);

            context.Users.Update(getUser);

            await context.SaveChangesAsync();

            return ResponseFactory.Success(mapper.UserToGetUserDto(getUser),
            "The data successfully updated");
        }

        private static bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private static bool AreAllFieldsFilled<T>(T user) where T : class
        {
            var properties = user.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.GetValue(user) is not string value)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
