using MinM_API.Dtos;
using MinM_API.Dtos.Discount;

namespace MinM_API.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<ServiceResponse<int>> AddDiscount(AddDiscountDto dto);
        Task<ServiceResponse<List<GetDiscountDto>>> GetAllDiscounts();
        Task<ServiceResponse<GetDiscountDto>> GetDiscountById(string id);
    }
}
