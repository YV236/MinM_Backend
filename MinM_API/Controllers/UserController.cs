using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Dtos;
using MinM_API.Dtos.RefreshToken;
using MinM_API.Dtos.User;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;
using System.Net.Security;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService _userService) : ControllerBase
    {
        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto userRegisterDto)
        {
            var response = await _userService.Register(userRegisterDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        [Route("UserInfo")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetUserInfo()
        {
            var response = await _userService.GetUserInfo(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        [Route("UpdateInfo")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> UpdateInfo(UpdateUserDto updateUserDto)
        {
            var response = await _userService.UpdateUserInfo(User, updateUserDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<TokenResponse>>> Login([FromBody] LoginDto model)
        {
            var response = await _userService.Login(model);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<ServiceResponse<TokenResponse>>> RefreshToken([FromBody] TokenRequest request)
        {
            var response = await _userService.RefreshToken(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Logout")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> Logout([FromBody] TokenRequest request)
        {
            var response = await _userService.Logout(request);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
