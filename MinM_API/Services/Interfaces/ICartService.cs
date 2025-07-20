using MinM_API.Dtos.Product;
using MinM_API.Dtos;
using System.Security.Claims;
using MinM_API.Dtos.Cart;

namespace MinM_API.Services.Interfaces
{
    public interface ICartService
    {
        Task<ServiceResponse<int>> AddProductToCart(ClaimsPrincipal user, AddCartItemDto cartItemDto);
        Task<ServiceResponse<List<GetCartItemDto>>> GetAllProductsFromCart(ClaimsPrincipal user);
        Task<ServiceResponse<int>> ActualizeUserCart(ClaimsPrincipal user, List<AddCartItemDto> productIds);
        Task<ServiceResponse<int>> DeleteProductFromCart(ClaimsPrincipal user, string cartItemId);
    }
}
