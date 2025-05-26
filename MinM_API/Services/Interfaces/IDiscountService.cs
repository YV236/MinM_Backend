using MinM_API.Dtos;
using MinM_API.Dtos.Discount;

namespace MinM_API.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<ServiceResponse<int>> AddDiscount(AddDiscountDto addDiscountDto);
        Task<ServiceResponse<List<GetDiscountDto>>> GetAllDiscounts();
        Task<ServiceResponse<GetDiscountDto>> GetDiscountById(string id);
        Task<ServiceResponse<int>> UpdateDiscount(UpdateDiscountDto updateDiscountDto);
    }
}
