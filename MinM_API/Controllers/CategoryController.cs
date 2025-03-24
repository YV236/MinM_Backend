using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Services.Interfaces;

namespace MinM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        [Route("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<GetCategoryDto>>>> GetAllCategories()
        {
            var response = await categoryService.GetAllCategory();

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<GetCategoryDto>>> AddCategory(AddCategoryDto addCategoryDto)
        {
            var response = await categoryService.AddCategory(addCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<GetCategoryDto>>> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            var response = await categoryService.UpdateCategory(updateCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteCategory(DeleteCategoryDto deleteCategoryDto)
        {
            var response = await categoryService.DeleteCategory(deleteCategoryDto);

            return StatusCode((int)response.StatusCode, response);
        }
    }
}
