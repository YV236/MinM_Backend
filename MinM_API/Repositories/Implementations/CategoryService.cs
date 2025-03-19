using Microsoft.EntityFrameworkCore;
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
                await context.SaveChangesAsync();

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

        public async Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto categoryDto)
        {
            var serviceResponse = new ServiceResponse<GetCategoryDto>();

            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.Id);

                if (category == null)
                {
                    serviceResponse.Data = new GetCategoryDto();
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There is no category with such id";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                if(category.Id == categoryDto.ParentCategoryId)
                {
                    throw new Exception("You cannot provide the same Id as the Parent Id for this category");
                }

                var parentCategory = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.ParentCategoryId);
                if (parentCategory == null)
                {
                    serviceResponse.Data = new GetCategoryDto();
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There is no category to be parent with such id";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                category!.Name = categoryDto.Name;
                category.Description = categoryDto.Description;
                category.ParentCategoryId = categoryDto.ParentCategoryId;

                context.Categories.Update(category);

                await context.SaveChangesAsync();

                var getCategoryDto = new GetCategoryDto()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                };

                serviceResponse.Data = getCategoryDto;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Category successfully updated";
                serviceResponse.StatusCode = HttpStatusCode.OK;

                return serviceResponse;
            }
            catch(Exception ex)
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
