using Azure;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Repositories.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(ICategoryService adminService) : ControllerBase
    {
        [HttpPost]
        [Route("AddCategory")]
        public async Task<ActionResult<ServiceResponse<GetCategoryDto>>> AddCategory(AddCategoryDto addCategoryDto)
        {
            var response = await adminService.AddCategory(addCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
