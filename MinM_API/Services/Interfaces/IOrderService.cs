using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using MinM_API.Models;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResponse<long>> SetOrderAsPaid(string orderId);
        Task<ServiceResponse<long>> CancelOrder(ClaimsPrincipal user, string orderId);
        Task<ServiceResponse<long>> CreateOrder(AddOrderDto addOrderDto, ClaimsPrincipal user);
        Task<ServiceResponse<long>> CreateUnauthorizedOrder(AddOrderDto addOrderDto);
        Task<ServiceResponse<long>> ChangeOrderStatus(string orderId, Status status);
        Task<ServiceResponse<List<OrderDto>>> GetAllOrders();
        Task<ServiceResponse<List<OrderDto>>> GetAllUserOrders(ClaimsPrincipal user);
        Task<ServiceResponse<OrderDto>> GetUserOrders(ClaimsPrincipal user, string orderId);
    }
}
