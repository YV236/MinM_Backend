using Azure;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Repositories.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<ServiceResponse<GetCategoryDto>>> AddCategory(AddCategoryDto addCategoryDto)
        {
            var response = await categoryService.AddCategory(addCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult<ServiceResponse<GetCategoryDto>>> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            var response = await categoryService.UpdateCategory(updateCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
