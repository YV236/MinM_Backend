using MinM_API.Dtos;
using MinM_API.Dtos.Category;

namespace MinM_API.Repositories.Interfaces
{
    public interface IAdminService
    {
        Task<ServiceResponse<GetCategoryDto>> AddCategory(AddCategoryDto categoryDto);
    }
}
