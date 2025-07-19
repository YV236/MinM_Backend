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
    public class WhisListController(IWishListService wishListService) : Controller
    {
        [HttpPost]
        [Route("AddProductToWishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> AddProductToWishList(string productId)
        {
            var response = await wishListService.AddProductToWishList(User, productId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetAllProductsFromWishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<List<GetProductDto>>>> GetAllProductsFromWhishList()
        {
            var response = await wishListService.GetAllProductsFromWishList(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetProductFromWishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<GetProductDto>>> GetProductFromWishList(string productId)
        {
            var response = await wishListService.GetProductFromWishList(User, productId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("UpdateWishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> ActualizeUserWishList(List<string> productIds)
        {
            var response = await wishListService.ActualizeUserWishList(User, productIds);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete]
        [Route("RemoveWishList")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteProductFromWishList(string whishListItemId)
        {
            var response = await wishListService.DeleteProductFromWishList(User, whishListItemId);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
