using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos.Category;
using MinM_API.Models;
using System.Net;

namespace MinM_API.Validators.Category
{
    public class CategoryValidationHelper(DataContext context)
    {
        public async Task EnsureCategoryExistsAsync(string categoryId, CancellationToken cancellationToken)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
            {
                throw new DtoValidationException(
                    "There is no category with such id", HttpStatusCode.NotFound);
            }
        }

        public async Task EnsureParentExistsAsync(
            string? parentCategoryId, CancellationToken cancellationToken)
        {
            if (parentCategoryId is not null &&
                !await context.Categories.AnyAsync(c => c.Id == parentCategoryId, cancellationToken))
            {
                throw new DtoValidationException(
                    "There is no category to be parent with such id", HttpStatusCode.NotFound);
            }
        }

        public async Task EnsureSiblingIsUniqueAsync(
            string? parentCategoryId,
            string name,
            string slug,
            string? excludedCategoryId,
            CancellationToken cancellationToken)
        {
            var siblings = context.Categories.Where(c =>
                c.ParentCategoryId == parentCategoryId && c.Id != excludedCategoryId);

            if (await siblings.AnyAsync(c => c.Name == name, cancellationToken))
            {
                throw SiblingConflict("name");
            }

            if (await siblings.AnyAsync(c => c.Slug == slug, cancellationToken))
            {
                throw SiblingConflict("slug");
            }
        }

        public async Task EnsureDeleteDoesNotCreateSiblingConflictsAsync(
            DeleteCategoryDto dto, CancellationToken cancellationToken)
        {
            var category = await context.Categories
                .AsNoTracking()
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

            if (category is null)
            {
                throw new DtoValidationException(
                    "There is no category with such id", HttpStatusCode.NotFound);
            }

            if (category.Subcategories is not { Count: > 0 } ||
                dto.Option == DeleteOption.CascadeDelete)
            {
                return;
            }

            var targetParentId = dto.Option == DeleteOption.Orphan
                ? null
                : category.ParentCategoryId;
            var children = category.Subcategories;

            if (children.GroupBy(c => c.Name).Any(group => group.Count() > 1))
            {
                throw ReassignmentConflict("name");
            }

            if (children.GroupBy(c => c.Slug).Any(group => group.Count() > 1))
            {
                throw ReassignmentConflict("slug");
            }

            var childIds = children.Select(c => c.Id).ToList();
            var childNames = children.Select(c => c.Name).ToList();
            var childSlugs = children.Select(c => c.Slug).ToList();
            var targetSiblings = context.Categories.AsNoTracking().Where(c =>
                c.ParentCategoryId == targetParentId &&
                c.Id != category.Id &&
                !childIds.Contains(c.Id));

            if (await targetSiblings.AnyAsync(c => childNames.Contains(c.Name), cancellationToken))
            {
                throw ReassignmentConflict("name");
            }

            if (await targetSiblings.AnyAsync(c => childSlugs.Contains(c.Slug), cancellationToken))
            {
                throw ReassignmentConflict("slug");
            }
        }

        private static DtoValidationException SiblingConflict(string field) =>
            new($"A category with the same {field} already exists under this parent",
                HttpStatusCode.Conflict);

        private static DtoValidationException ReassignmentConflict(string field) =>
            new($"Cannot move subcategories because the target level already contains the same {field}",
                HttpStatusCode.Conflict);
    }
}
