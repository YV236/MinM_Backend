using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Migrations;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class CategoryService(DataContext context, CategoryMapper mapper) : ICategoryService
    {
        public async Task<ServiceResponse<List<GetCategoryDto>>> GetAllCategory()
        {
            try
            {
                var CategoryList = await context.Categories!
                    .OrderBy(c=>c.Name)
                    .ToListAsync();

                if (CategoryList == null || CategoryList.Count == 0)
                {
                    return ResponseFactory.Error(new List<GetCategoryDto>(), "There are no categories", HttpStatusCode.NotFound);
                }

                var getCategoryDtoList = new List<GetCategoryDto>();

                foreach (var category in CategoryList)
                {
                    var getCategory = mapper.CategoryToGetCategoryDto(category);

                    getCategoryDtoList.Add(getCategory);
                }

                return ResponseFactory.Success(getCategoryDtoList, "Successful extraction of categories");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(new List<GetCategoryDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto categoryDto)
        {
            try
            {
                var category = new Category()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = categoryDto.Name,
                    Slug = SlugExtension.GenerateSlug(categoryDto.Name),
                    Description = categoryDto.Description,
                    ParentCategoryId = categoryDto.ParentCategoryId,
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync();

                var getCategoryDto = mapper.CategoryToGetCategoryDto(category);

                return ResponseFactory.Success(getCategoryDto, "Category successfully created");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(new GetCategoryDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto categoryDto)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.Id);

                if (category == null)
                {
                    return ResponseFactory.Error(new GetCategoryDto(), "There is no category with such id", HttpStatusCode.NotFound);
                }

                if (category.Id == categoryDto.ParentCategoryId)
                {
                    return ResponseFactory.Error(new GetCategoryDto(), "You cannot provide the same Id as the Parent Id for this category");
                }

                var parentCategory = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.ParentCategoryId);

                if (categoryDto.ParentCategoryId != null && parentCategory == null)
                {
                    return ResponseFactory.Error(new GetCategoryDto(), "There is no category to be parent with such id", HttpStatusCode.NotFound);
                }

                mapper.UpdateCategoryDtoToCategory(categoryDto, category);
                category.Slug = SlugExtension.GenerateSlug(categoryDto.Name);

                context.Categories.Update(category);

                await context.SaveChangesAsync();

                var getCategoryDto = mapper.CategoryToGetCategoryDto(category);

                return ResponseFactory.Success(getCategoryDto, "Category successfully updated");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(new GetCategoryDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteCategory(DeleteCategoryDto categoryDto)
        {
            try
            {
                var category = await context.Categories
                    .Include(c => c.Subcategories)
                    .FirstOrDefaultAsync(c => c.Id == categoryDto.CategoryId);

                if (category == null)
                {
                    return ResponseFactory.Error(0, "There is no category with such id", HttpStatusCode.NotFound);
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

                return ResponseFactory.Success(1, "Category successfully removed");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
