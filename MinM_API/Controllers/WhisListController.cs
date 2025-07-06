using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhisListController(IWhishListService whishListService) : Controller
    {
        [HttpPost]
        [Route("AddProductToWhishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> AddProductToWishList(string productId)
        {
            var response = await whishListService.AddProductToWishList(User, productId);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
