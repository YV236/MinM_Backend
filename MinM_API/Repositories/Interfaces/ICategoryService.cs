using MinM_API.Dtos;
using MinM_API.Dtos.Category;

namespace MinM_API.Repositories.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto categoryDto);
        Task<ServiceResponse<GetCategoryDto>> UpdateCategory(UpdateCategoryDto categoryDto);
    }
}
