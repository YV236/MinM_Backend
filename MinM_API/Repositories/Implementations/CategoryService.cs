using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using System.Net;

namespace MinM_API.Repositories.Implementations
{
    public class CategoryService(DataContext context) : ICategoryService
    {
        public async Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto categoryDto)
        {
            var serviceResponse = new ServiceResponse<GetCategoryDto>();

            try
            {
                var category = new Category()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = categoryDto.Name,
                    Description = categoryDto.Description,
                    ParentCategoryId = categoryDto.ParentCategoryId,
                };

                context.Categories.Add(category);
                context.SaveChanges();

                var getCategoryDto = new GetCategoryDto()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                };

                serviceResponse.Data = getCategoryDto;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Category successfully created";
                serviceResponse.StatusCode = HttpStatusCode.OK;

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = new GetCategoryDto();
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }
        }
    }
}
