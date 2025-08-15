using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class OrderService : IOrderService
    {
        public Task<ServiceResponse<int>> CreateOrder(AddOrderDto addOrderDto)
        {
            throw new NotImplementedException();
        }
    }
}
