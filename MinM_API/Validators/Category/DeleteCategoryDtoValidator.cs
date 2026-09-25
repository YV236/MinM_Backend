using MinM_API.Dtos.Category;

namespace MinM_API.Validators.Category
{
    public class DeleteCategoryDtoValidator(CategoryValidationHelper helper)
        : DtoValidator<DeleteCategoryDto>
    {
        protected override async Task ValidateAsync(
            DeleteCategoryDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryId))
            {
                throw new DtoValidationException("Category id is required");
            }

            if (!Enum.IsDefined(dto.Option))
            {
                throw new DtoValidationException("Invalid category delete option");
            }

            await helper.EnsureDeleteDoesNotCreateSiblingConflictsAsync(dto, cancellationToken);
        }
    }
}
