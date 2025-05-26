using MinM_API.Dtos;
using MinM_API.Dtos.Category;

namespace MinM_API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<List<GetCategoryDto>>> GetAllCategory();
        Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto addCategoryDto);
        Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto updateCategoryDto);
        Task<ServiceResponse<int>> DeleteCategory(DeleteCategoryDto deleteCategoryDto);
    }
}
