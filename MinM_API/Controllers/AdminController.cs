using Microsoft.AspNetCore.Mvc;
using MinM_API.Repositories.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
    }
}
