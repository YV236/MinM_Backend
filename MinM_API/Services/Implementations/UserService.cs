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
    public class UserService(IUserRepository userRepository, UserManager<User> userManager,
        DataContext context, UserMapper mapper, ILogger<UserService> logger) : IUserService
    {
        public async Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {
            var emptyFields = GetEmptyStringFields(userRegisterDto);

            if (emptyFields.Any())
            {
                var fieldList = string.Join(", ", emptyFields);
                logger.LogInformation($"Fail: Registration error. The following fields are missing or invalid: {fieldList}");
                return ResponseFactory.Error(0,
                    "Error while registering. Some of the properties may be filled incorrectly",
                    HttpStatusCode.UnprocessableEntity);
            }

            if (!IsValidEmail(userRegisterDto.Email))
            {
                logger.LogInformation("Fail: 'Email' field is missing or not in correct format. Email: {Email}", userRegisterDto.Email);
                return ResponseFactory.Error(0,
                    $"Registration failed. The email '{userRegisterDto.Email}' must include '@' and a domain such as '.com' or '.pl'.",
                    HttpStatusCode.UnprocessableEntity);
            }

            if (userRegisterDto.PhoneNumber.Any(c => !char.IsDigit(c)) || userRegisterDto.PhoneNumber.Length < 9)
            {
                logger.LogInformation("Fail: 'Phone number' field is missing or not in correct format. Phone number: {Number}", userRegisterDto.PhoneNumber);
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
                    logger.LogInformation("Fail: Fail while creating User. Message: {Message}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return ResponseFactory.Error(0, "Failed to create user");
                }

                var addRoleResult = await userManager.AddToRoleAsync(user, "User");

                if (!addRoleResult.Succeeded)
                {
                    var errorDescriptions = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));

                    logger.LogError("Fail: Failed to assign role 'User' to user {UserId}. Message: {Message}", user.Id, errorDescriptions);

                    return ResponseFactory.Error(0, "Failed to assign role to user. Please contact support.");
                }

                return ResponseFactory.Success(1, "User registered successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while registration. Input data: {@UserRegisterDto}", userRegisterDto);
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
                logger.LogError(ex, "Fail: Error while to retrieving products from database");
                return ResponseFactory.Error(new GetUserDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetUserDto>> UpdateUserInfo(ClaimsPrincipal user, UpdateUserDto userUpdateDto)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);

                if (getUser == null)
                {
                    return ResponseFactory.Error(new GetUserDto(), "User not found.", HttpStatusCode.NotFound);
                }

                var emptyFields = GetEmptyStringFields(userUpdateDto);

                if (emptyFields.Any())
                {
                    var fieldList = string.Join(", ", emptyFields);
                    logger.LogInformation($"Fail: The following fields are missing or invalid: {fieldList}");

                    return ResponseFactory.Error(new GetUserDto(),
                        "Error while updating data. Some of the properties may be filled incorrectly",
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while updating data. Input data: {@UserUpdaterDto}", userUpdateDto);
                return ResponseFactory.Error(new GetUserDto(), "Internal error");
            }
        }

        private static bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private static List<string> GetEmptyStringFields<T>(T obj) where T : class
        {
            var emptyFields = new List<string>();

            var properties = obj.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (property.GetValue(obj) is not string value)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    emptyFields.Add(property.Name);
                }
            }

            return emptyFields;
        }
    }
}
