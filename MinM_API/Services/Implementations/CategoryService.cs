using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Category;
using MinM_API.Dtos.Product;
using MinM_API.Extension;
using MinM_API.Mappers;

using MinM_API.Models;
using MinM_API.Services.Interfaces;
using Npgsql;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class CategoryService(DataContext context, CategoryMapper mapper,
        ILogger<CategoryService> logger, IPhotoService photoService) : ICategoryService
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
                logger.LogError(ex, "Fail: Error while retrieving categories from database");
                return ResponseFactory.Error(new List<GetCategoryDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto addCategoryDto)
        {
            try
            {
                if (addCategoryDto.ParentCategoryId is not null &&
                    !await context.Categories.AnyAsync(c => c.Id == addCategoryDto.ParentCategoryId))
                {
                    return ResponseFactory.Error(new GetCategoryDto(),
                        "There is no category to be parent with such id", HttpStatusCode.NotFound);
                }

                var slug = SlugExtension.GenerateSlug(addCategoryDto.Name);
                var conflict = await GetSiblingConflictAsync(
                    addCategoryDto.ParentCategoryId, addCategoryDto.Name, slug);

                if (conflict is not null)
                {
                    return CategoryConflict(new GetCategoryDto(), conflict);
                }

                var category = new Category()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addCategoryDto.Name,
                    Slug = slug,
                    Description = addCategoryDto.Description,
                    ParentCategoryId = addCategoryDto.ParentCategoryId,
                    ImageURL = await photoService.UploadImageAsync(addCategoryDto.Image)
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync();

                var getCategoryDto = mapper.CategoryToGetCategoryDto(category);

                return ResponseFactory.Success(getCategoryDto, "Category successfully created");
            }
            catch (DbUpdateException ex) when (IsCategoryUniquenessViolation(ex))
            {
                logger.LogInformation(ex, "Category sibling uniqueness conflict while adding {CategoryName}", addCategoryDto.Name);
                return CategoryConflict(new GetCategoryDto(), "name or slug");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding category. Name: {CategoryName}", addCategoryDto.Name);
                return ResponseFactory.Error(new GetCategoryDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == updateCategoryDto.Id);

                if (category == null)
                {
                    logger.LogInformation("Fail: No categories found in database");
                    return ResponseFactory.Error(new GetCategoryDto(), "There is no category with such id", HttpStatusCode.NotFound);
                }

                if (category.Id == updateCategoryDto.ParentCategoryId)
                {
                    logger.LogInformation("Fail: You can't provide same Id as the Parent Id for this category. Id: {CategoryId}",
                        updateCategoryDto.ParentCategoryId);
                    return ResponseFactory.Error(new GetCategoryDto(), "You can not provide the same Id as the Parent Id for this category");
                }

                var parentCategory = await context.Categories.FirstOrDefaultAsync(c => c.Id == updateCategoryDto.ParentCategoryId);

                if (updateCategoryDto.ParentCategoryId != null && parentCategory == null)
                {
                    logger.LogInformation("Fail: There is no category with such id. Id: {CategoryId}", category.ParentCategoryId);
                    return ResponseFactory.Error(new GetCategoryDto(), "There is no category to be parent with such id", HttpStatusCode.NotFound);
                }

                var slug = SlugExtension.GenerateSlug(updateCategoryDto.Name);
                var conflict = await GetSiblingConflictAsync(
                    updateCategoryDto.ParentCategoryId, updateCategoryDto.Name, slug, category.Id);

                if (conflict is not null)
                {
                    return CategoryConflict(new GetCategoryDto(), conflict);
                }

                mapper.UpdateCategoryDtoToCategory(updateCategoryDto, category);
                category.Slug = slug;

                if (updateCategoryDto.NewImage != null)
                {
                    var publicId = photoService.GetPublicIdFromUrl(category.ImageURL);
                    await photoService.DeleteImageAsync(publicId);
                    category.ImageURL = await photoService.UploadImageAsync(updateCategoryDto.NewImage);
                }

                context.Categories.Update(category);

                await context.SaveChangesAsync();

                var getCategoryDto = mapper.CategoryToGetCategoryDto(category);

                return ResponseFactory.Success(getCategoryDto, "Category successfully updated");
            }
            catch (DbUpdateException ex) when (IsCategoryUniquenessViolation(ex))
            {
                logger.LogInformation(ex, "Category sibling uniqueness conflict while updating {CategoryId}", updateCategoryDto.Id);
                return CategoryConflict(new GetCategoryDto(), "name or slug");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error updating category. Name: {CategoryName}, Id: {CategoryId}",
                    updateCategoryDto.Name, updateCategoryDto.Id);
                return ResponseFactory.Error(new GetCategoryDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteCategory(DeleteCategoryDto deleteCategoryDto)
        {
            try
            {
                var category = await context.Categories
                    .Include(c => c.Subcategories)
                    .FirstOrDefaultAsync(c => c.Id == deleteCategoryDto.CategoryId);

                if (category == null)
                {
                    logger.LogInformation("Fail: No categories found in database. Id: {CategoryId}", deleteCategoryDto.CategoryId);
                    return ResponseFactory.Error(0, "There is no category with such id", HttpStatusCode.NotFound);
                }

                if (category.ParentCategoryId == null && deleteCategoryDto.Option == DeleteOption.ReassignToParent)
                {
                    deleteCategoryDto.Option = DeleteOption.Orphan;
                }

                if (category.Subcategories!.Count != 0 &&
                    deleteCategoryDto.Option is DeleteOption.ReassignToParent or DeleteOption.Orphan)
                {
                    var targetParentId = deleteCategoryDto.Option == DeleteOption.Orphan
                        ? null
                        : category.ParentCategoryId;
                    var conflict = await GetReassignmentConflictAsync(
                        category.Subcategories, targetParentId, category.Id);

                    if (conflict is not null)
                    {
                        return ResponseFactory.Error(0,
                            $"Cannot move subcategories because the target level already contains the same {conflict}",
                            HttpStatusCode.Conflict);
                    }
                }

                var publicId = photoService.GetPublicIdFromUrl(category.ImageURL);
                await photoService.DeleteImageAsync(publicId);

                if (category.Subcategories!.Count != 0)
                {
                    switch (deleteCategoryDto.Option)
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
            catch (DbUpdateException ex) when (IsCategoryUniquenessViolation(ex))
            {
                logger.LogInformation(ex, "Category sibling uniqueness conflict while deleting {CategoryId}", deleteCategoryDto.CategoryId);
                return CategoryConflict(0, "name or slug");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while deleting category. CategoryId: {CategoryId}", deleteCategoryDto.CategoryId);
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        private async Task<string?> GetSiblingConflictAsync(
            string? parentCategoryId, string name, string slug, string? excludedCategoryId = null)
        {
            var siblings = context.Categories.Where(c =>
                c.ParentCategoryId == parentCategoryId && c.Id != excludedCategoryId);

            if (await siblings.AnyAsync(c => c.Name == name))
            {
                return "name";
            }

            return await siblings.AnyAsync(c => c.Slug == slug) ? "slug" : null;
        }

        private async Task<string?> GetReassignmentConflictAsync(
            List<Category> children, string? targetParentId, string deletedCategoryId)
        {
            if (children.GroupBy(c => c.Name).Any(g => g.Count() > 1))
            {
                return "name";
            }

            if (children.GroupBy(c => c.Slug).Any(g => g.Count() > 1))
            {
                return "slug";
            }

            var childIds = children.Select(c => c.Id).ToList();
            var childNames = children.Select(c => c.Name).ToList();
            var childSlugs = children.Select(c => c.Slug).ToList();
            var targetSiblings = context.Categories.Where(c =>
                c.ParentCategoryId == targetParentId &&
                c.Id != deletedCategoryId &&
                !childIds.Contains(c.Id));

            if (await targetSiblings.AnyAsync(c => childNames.Contains(c.Name)))
            {
                return "name";
            }

            return await targetSiblings.AnyAsync(c => childSlugs.Contains(c.Slug)) ? "slug" : null;
        }

        private static ServiceResponse<T> CategoryConflict<T>(T data, string field) =>
            ResponseFactory.Error(data,
                $"A category with the same {field} already exists under this parent",
                HttpStatusCode.Conflict);

        private static bool IsCategoryUniquenessViolation(DbUpdateException exception) =>
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_Categories_ParentCategoryId_Name" or
                    "UX_Categories_Root_Name" or
                    "UX_Categories_ParentCategoryId_Slug" or
                    "UX_Categories_Root_Slug"
            };
    }
}
