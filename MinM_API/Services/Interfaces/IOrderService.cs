using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResponse<int>> CreateOrder(AddOrderDto addOrderDto, ClaimsPrincipal user);
    }
}
