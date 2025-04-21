using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class CategoryService(DataContext context) : ICategoryService
    {
        public async Task<ServiceResponse<List<GetCategoryDto>>> GetAllCategory()
        {
            var serviceResponse = new ServiceResponse<List<GetCategoryDto>>();

            try
            {
                var unsortedCategoryList = await context.Categories!.ToListAsync();

                if (unsortedCategoryList == null || unsortedCategoryList.Count == 0)
                {
                    serviceResponse.Data = [];
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There are no categories";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                //var rootCategories = unsortedCategoryList
                //    .Where(c => c.ParentCategoryId == null)
                //    .OrderBy(c => c.Name)
                //    .ToList();

                var getCategoryDtoList = new List<GetCategoryDto>();

                foreach (var category in unsortedCategoryList)
                {
                    var getCategory = ConvertToDto(category, unsortedCategoryList);
                    getCategoryDtoList.Add(getCategory);
                }

                serviceResponse.Data = getCategoryDtoList;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful extraction of categories";
                serviceResponse.StatusCode = HttpStatusCode.OK;

                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = [];
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }
        }

        private static GetCategoryDto ConvertToDto(Category category, List<Category> allCategories)
        {
            var dto = new GetCategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description!,
                ParentCategoryId = category.ParentCategoryId,
                //SubCategories = []
            };

            //var subCategories = allCategories
            //    .Where(c => c.ParentCategoryId == category.Id)
            //    .OrderBy(c => c.Name)
            //    .ToList();

            //foreach (var subCategory in subCategories)
            //{
            //    dto.SubCategories.Add(ConvertToDto(subCategory, allCategories));
            //}

            return dto;
        }

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

                if (category.Id == categoryDto.ParentCategoryId)
                {
                    throw new Exception("You cannot provide the same Id as the Parent Id for this category");
                }

                var parentCategory = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.ParentCategoryId);

                if (categoryDto.ParentCategoryId != null && parentCategory == null)
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
            catch (Exception ex)
            {
                serviceResponse.Data = new GetCategoryDto();
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }
        }

        public async Task<ServiceResponse<int>> DeleteCategory(DeleteCategoryDto categoryDto)
        {
            var serviceResponse = new ServiceResponse<int>();

            try
            {
                var category = await context.Categories
                    .Include(c => c.Subcategories)
                    .FirstOrDefaultAsync(c => c.Id == categoryDto.CategoryId);

                if (category == null)
                {
                    serviceResponse.Data = 0;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There is no category with such id";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                if (category.ParentCategoryId == null && categoryDto.Option == DeleteOption.ReassignToParent)
                    categoryDto.Option = DeleteOption.Orphan;

                if (category.Subcategories!.Count != 0)
                {
                    switch (categoryDto.Option)
                    {
                        case DeleteOption.CascadeDelete:
                            context.Categories.RemoveRange(category.Subcategories!);
                            break;

                        case DeleteOption.ReassignToParent:
                            foreach (var child in category.Subcategories!)
                            {
                                child.ParentCategoryId = category.ParentCategoryId;
                            }
                            break;

                        case DeleteOption.Orphan:
                            foreach (var child in category.Subcategories!)
                            {
                                child.ParentCategoryId = null;
                            }
                            break;
                    }
                }

                context.Categories.Remove(category);
                await context.SaveChangesAsync();

                serviceResponse.Data = 1;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Category successfully removed";
                serviceResponse.StatusCode = HttpStatusCode.OK;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
                return serviceResponse;
            }
        }
    }
}
