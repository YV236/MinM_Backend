using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(IUserService _userService) : ControllerBase
    {
    }
}
