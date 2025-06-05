using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;
using MinM_API.Dtos.Season;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController(ISeasonService seasonService) : ControllerBase
    {
        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<int>>> AddSeason(AddSeasonDto seasonDto)
        {
            var response = await seasonService.AddSeason(seasonDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
