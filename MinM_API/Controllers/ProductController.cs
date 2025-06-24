using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(IProductService productService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> AddProduct([FromForm] AddProductDto productDto)
        {
            var response = await productService.AddProduct(productDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> UpdateProduct(UpdateProductDto productDto)
        {
            var response = await productService.UpdateProduct(productDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = "MyTokenScheme")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteProduct(string id)
        {
            var response = await productService.DeleteProduct(id);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<GetProductDto>>>> GetAllProducts()
        {
            var response = await productService.GetAllProducts();

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<ServiceResponse<GetProductDto>>> GetProductById(string id)
        {
            var response = await productService.GetProductById(id);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet]
        [Route("{slug}")]
        public async Task<ActionResult<ServiceResponse<GetProductDto>>> GetProductBySlug(string slug)
        {
            var response = await productService.GetProductBySlug(slug);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
