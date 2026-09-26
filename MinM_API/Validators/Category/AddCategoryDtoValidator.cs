using MinM_API.Dtos.Category;
using MinM_API.Extension;

namespace MinM_API.Validators.Category
{
    public class AddCategoryDtoValidator(CategoryValidationHelper helper)
        : DtoValidator<AddCategoryDto>
    {
        protected override async Task ValidateAsync(
            AddCategoryDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new DtoValidationException("Category name is required");
            }

            if (dto.Image is null)
            {
                throw new DtoValidationException("Category image is required");
            }

            var slug = SlugExtension.GenerateSlug(dto.Name);
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new DtoValidationException("Category name must produce a valid slug");
            }

            await helper.EnsureParentExistsAsync(dto.ParentCategoryId, cancellationToken);
            await helper.EnsureSiblingIsUniqueAsync(
                dto.ParentCategoryId, dto.Name, slug, null, cancellationToken);
        }
    }
}
