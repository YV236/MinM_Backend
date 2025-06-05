using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController(ISeasonService seasonService) : ControllerBase
    {
    }
}
