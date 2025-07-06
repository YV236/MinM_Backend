using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using System.Security.Claims;

namespace MinM_API.Services.Interfaces
{
    public interface IWhishListService
    {
        Task<ServiceResponse<int>> AddProductToWishList(ClaimsPrincipal user, string productId);
    }
}
