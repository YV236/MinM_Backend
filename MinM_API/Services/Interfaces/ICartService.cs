using MinM_API.Dtos.Product;
using MinM_API.Dtos;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface ICartService
    {
        Task<ServiceResponse<int>> AddProductToCart(ClaimsPrincipal user, string productId);
        Task<ServiceResponse<List<GetProductDto>>> GetAllProductsFromCart(ClaimsPrincipal user);
        Task<ServiceResponse<GetProductDto>> GetProductFromCart(ClaimsPrincipal user, string cartItemId);
        Task<ServiceResponse<int>> ActualizeUserCart(ClaimsPrincipal user, List<string> productIds);
        Task<ServiceResponse<int>> DeleteProductFromCart(ClaimsPrincipal user, string cartItemId);
    }
}
