using MinM_API.Dtos.Category;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class CategoryMapper
    {
        public partial GetCategoryDto CategoryToGetCategoryDto(Category category);

        [MapperIgnoreSource(nameof(Category.Id))]
        [MapperIgnoreSource(nameof(Category.Subcategories))]
        [MapperIgnoreSource(nameof(Category.Products))]
        public partial void UpdateCategoryDtoToCategory(UpdateCategoryDto categoryDto, Category category);
    }
}
