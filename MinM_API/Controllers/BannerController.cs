using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using MinM_API.Dtos;
using MinM_API.Dtos.Banner;
using MinM_API.Services.Interfaces;
using System.Diagnostics.Contracts;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController(IBannerService bannerService) : ControllerBase
    {
        [HttpPut]
        [Route("UpdateBanner")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> UpdateBanners([FromForm] AddBannerImagesDto addBannerImages)
        {
            var result = await bannerService.UpdateBanners(addBannerImages);

            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet]
        [Route("GetBannerImages")]
        public async Task<ActionResult<ServiceResponse<List<GetBannerImagesDto>>>> GetBannerImages()
        {
            var result = await bannerService.GetBannerImages();

            return StatusCode((int)result.StatusCode, result);
        }

    }
}
