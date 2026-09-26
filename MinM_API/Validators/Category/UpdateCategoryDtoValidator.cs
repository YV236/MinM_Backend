using MinM_API.Dtos.Category;
using MinM_API.Extension;

namespace MinM_API.Validators.Category
{
    public class UpdateCategoryDtoValidator(CategoryValidationHelper helper)
        : DtoValidator<UpdateCategoryDto>
    {
        protected override async Task ValidateAsync(
            UpdateCategoryDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                throw new DtoValidationException("Category id is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new DtoValidationException("Category name is required");
            }

            if (dto.Id == dto.ParentCategoryId)
            {
                throw new DtoValidationException(
                    "You can not provide the same Id as the Parent Id for this category");
            }

            var slug = SlugExtension.GenerateSlug(dto.Name);
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new DtoValidationException("Category name must produce a valid slug");
            }

            await helper.EnsureCategoryExistsAsync(dto.Id, cancellationToken);
            await helper.EnsureParentExistsAsync(dto.ParentCategoryId, cancellationToken);
            await helper.EnsureSiblingIsUniqueAsync(
                dto.ParentCategoryId, dto.Name, slug, dto.Id, cancellationToken);
        }
    }
}
