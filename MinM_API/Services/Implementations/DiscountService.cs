using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class DiscountService : IDiscountService
    {
        public Task<ServiceResponse<int>> AddDiscount(AddDiscountDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
