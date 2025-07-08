using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IWhishListService
    {
        Task<ServiceResponse<int>> AddProductToWishList(ClaimsPrincipal user, string productId);
        Task<ServiceResponse<List<GetProductDto>>> GetAllProductsFromWhishList(ClaimsPrincipal user);
        Task<ServiceResponse<GetProductDto>> GetProductFromWishList(ClaimsPrincipal user, string whishListItemId);
        Task<ServiceResponse<int>> ActualizeUserWishList(ClaimsPrincipal user, List<string> productIds);
    }
}
