using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController(ICartService cartService) : ControllerBase
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> ActualizeUserCart(List<string> productIds)
        {
            var response = await cartService.ActualizeUserCart(User, productIds);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> GetAllProductsFromCart()
        {
            var response = await cartService.GetAllProductsFromCart(User);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> GetProductFromCart(string productIds)
        {
            var response = await cartService.GetProductFromCart(User, productIds);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteProductFromCart(string productIds)
        {
            var response = await cartService.DeleteProductFromCart(User, productIds);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> AddProductToCart(string productIds)
        {
            var response = await cartService.AddProductToCart(User, productIds);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
