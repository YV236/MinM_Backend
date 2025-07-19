using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Cart;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemController(ICartService cartService) : ControllerBase
    {
        [HttpPut]
        [Route("UpdateCart")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<GetCartItemDto>>> ActualizeUserCart(List<AddCartItemDto> items)
        {
            var response = await cartService.ActualizeUserCart(User, items);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost]
        [Route("AddProductToCart")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<GetCartItemDto>>> AddProductToCart(AddCartItemDto item)
        {
            var response = await cartService.AddProductToCart(User, item);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete]
        [Route("DeleteProductFromCart")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteProductFromCart(string itemId)
        {
            var response = await cartService.DeleteProductFromCart(User, itemId);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetAllCartProducts")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<GetCartItemDto>>> GetAllProductsFromCart()
        {
            var response = await cartService.GetAllProductsFromCart(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetProductFromCart")]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<GetCartItemDto>>> GetProductFromCart(string itemId)
        {
            var response = await cartService.GetProductFromCart(User, itemId);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
