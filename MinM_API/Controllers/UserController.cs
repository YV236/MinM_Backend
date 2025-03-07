using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos.User;
using MinM_API.Models;
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
    }
}
