using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.User;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [Authorize]
        [Route("UserInfo")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetUserInfo()
        {
            var response = await _userService.GetUserInfo(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(Roles = "User")]
        [Route("UpdateInfo")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> UpdateInfo(UpdateUserDto updateUserDto)
        {
            var response = await _userService.UpdateUserInfo(User, updateUserDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
