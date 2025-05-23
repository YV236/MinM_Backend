using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Dtos.Products;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Migrations;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class CategoryService(DataContext context, CategoryMapper mapper, ILogger<CategoryService> logger) : ICategoryService
    {
        public async Task<ServiceResponse<List<GetCategoryDto>>> GetAllCategory()
        {
            try
            {
                var categoryList = await context.Categories!
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                if (categoryList == null || categoryList.Count == 0)
                {
                    logger.LogInformation("Fail: No categories found in database");
                    return ResponseFactory.Error(new List<GetCategoryDto>(), "There are no categories", HttpStatusCode.NotFound);
                }

                var getCategoryDtoList = categoryList
                   .Select(c => mapper.CategoryToGetCategoryDto(c))
                   .ToList();

                return ResponseFactory.Success(getCategoryDtoList, "Successful extraction of categories");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Failed to retrieve categories from database");
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
                logger.LogError(ex, "Fail: Failed to add category. Name: {CategoryName}", categoryDto.Name);
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
                    logger.LogInformation("Fail: No categories found in database");
                    return ResponseFactory.Error(new GetCategoryDto(), "There is no category with such id", HttpStatusCode.NotFound);
                }

                if (category.Id == categoryDto.ParentCategoryId)
                {
                    logger.LogInformation("Fail: You can't provide same Id as the Parent Id for this category. Id: {CategoryId}",
                        categoryDto.ParentCategoryId);
                    return ResponseFactory.Error(new GetCategoryDto(), "You can not provide the same Id as the Parent Id for this category");
                }

                var parentCategory = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryDto.ParentCategoryId);

                if (categoryDto.ParentCategoryId != null && parentCategory == null)
                {
                    logger.LogInformation("Fail: There is no category with such id. Id: {CategoryId}", category.ParentCategoryId);
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
                logger.LogError(ex, "Fail: Failed to update category. Name: {CategoryName}, Id: {CategoryId}",
                    categoryDto.Name, categoryDto.Id);
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
                    logger.LogInformation("Fail: No categories found in database. Id: {CategoryId}", categoryDto.CategoryId);
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
                logger.LogError(ex, "Fail: Failed to delete category. CategoryId: {CategoryId}", categoryDto.CategoryId);
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
