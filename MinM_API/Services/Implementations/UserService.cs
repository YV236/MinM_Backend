using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.RefreshToken;
using MinM_API.Dtos.User;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using MinM_API.Services.Interfaces;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;

using AddressModel = MinM_API.Models.Address;

namespace MinM_API.Services.Implementations
{
    public class UserService(IUserRepository userRepository, UserManager<User> userManager,
        DataContext context, UserMapper mapper, ILogger<UserService> logger,
        JwtTokenService jwtTokenService) : IUserService
    {
        public async Task<ServiceResponse<TokenResponse>> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return ResponseFactory.Error(new TokenResponse(),
                    "Error while registering. Some of the properties may be filled incorrectly",
                    HttpStatusCode.UnprocessableEntity);
            }

            var roles = await userManager.GetRolesAsync(user);

            return ResponseFactory.Success(await jwtTokenService.CreateUserTokenAsync(user, roles), "User logged in successfully");
        }


        public async Task<ServiceResponse<int>> Register(UserRegisterDto userRegisterDto)
        {
            var emptyFields = GetEmptyStringFields(userRegisterDto);

            if (emptyFields.Count > 0)
            {
                var fieldList = string.Join(", ", emptyFields);
                logger.LogInformation("Fail: Registration error. The following fields are missing or invalid: {FieldList}", fieldList);
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

            if (!IsValidPassword(userRegisterDto.Password))
            {
                logger.LogInformation("Fail: 'Password' field is missing or not in correct format. Password: {Password}", userRegisterDto.Password);
                return ResponseFactory.Error(0,
                    $"Registration failed. The password '{userRegisterDto.Password}' must include special symbols numbers and uppercase.",
                    HttpStatusCode.UnprocessableEntity);
            }

            try
            {
                var user = new User
                {
                    Email = userRegisterDto.Email,
                    UserName = userRegisterDto.Email,
                    DateOfCreation = DateTime.Now,
                };

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

                if (emptyFields.Count > 0)
                {
                    var fieldList = string.Join(", ", emptyFields);
                    logger.LogInformation("Fail: The following fields are missing or invalid: {FieldList}", fieldList);
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

                if (getUser.Address is null)
                {
                    getUser.Address = new AddressModel() { Id = Guid.NewGuid().ToString() };
                    getUser.AddressId = getUser.Address.Id;
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

        public async Task<ServiceResponse<TokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                var principal = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    logger.LogWarning("Invalid access token provided for refresh");
                    return ResponseFactory.Error(new TokenResponse(),
                        "Invalid access token",
                        HttpStatusCode.BadRequest);
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseFactory.Error(new TokenResponse(),
                        "Invalid token claims",
                        HttpStatusCode.BadRequest);
                }

                var user = await jwtTokenService.GetUserByRefreshTokenAsync(request.RefreshToken);
                if (user == null || user.Id != userId)
                {
                    logger.LogWarning("Invalid refresh token for user {UserId}", userId);
                    return ResponseFactory.Error(new TokenResponse(),
                        "Invalid refresh token",
                        HttpStatusCode.Unauthorized);
                }

                await jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

                var roles = await userManager.GetRolesAsync(user);
                var newTokenResponse = await jwtTokenService.CreateUserTokenAsync(user, roles);

                logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

                return ResponseFactory.Success(newTokenResponse, "Token refreshed successfully");
            }
            catch (SecurityTokenException ex)
            {
                logger.LogWarning(ex, "Security token exception during refresh");
                return ResponseFactory.Error(new TokenResponse(),
                    "Invalid token",
                    HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during token refresh");
                return ResponseFactory.Error(new TokenResponse(),
                    "Token refresh failed",
                    HttpStatusCode.InternalServerError);
            }
        }

        private static bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private static bool IsValidPassword(string password)
        {
            var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";
            return Regex.IsMatch(password, passwordPattern);
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
