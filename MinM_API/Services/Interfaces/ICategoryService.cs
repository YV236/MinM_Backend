using MinM_API.Dtos;
using MinM_API.Dtos.Category;

namespace MinM_API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<List<GetCategoryDto>>> GetAllCategory();
        Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto categoryDto);
        Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto categoryDto);
        Task<ServiceResponse<int>> DeleteCategory(DeleteCategoryDto categoryDto);
    }
}
