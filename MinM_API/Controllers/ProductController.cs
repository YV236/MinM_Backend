using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Products;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<ServiceResponse<int>>> AddProduct(AddProductDto productDto)
        {
            var response = await productService.AddProduct(productDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
