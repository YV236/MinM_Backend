using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class WhishListService(DataContext context, ILogger<WhishListService> logger, ProductMapper mapper) : IWhishListService
    {
        public async Task<ServiceResponse<int>> AddProductToWishList(ClaimsPrincipal user, string productId)
        {
            try
            {
                var whishListItem = new WishlistItem
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                    ProductId = productId,
                    AddedAt = DateTime.Now,
                };

                context.WishlistItems.Add(whishListItem);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Product successfully added to whishList");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding product to whishList");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProductsFromWhishList(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                if (userId.IsNullOrEmpty())
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(new List<GetProductDto>(), "There is no such user with such id");
                }

                var userProducts = context.WishlistItems
                    .Where(wi => wi.UserId == userId)
                    .Select(wi => wi.Product)
                    .ToList();

                var getProductList = new List<GetProductDto>();

                foreach (var product in userProducts)
                {
                    getProductList.Add(mapper.ProductToGetProductDto(product));
                }

                return ResponseFactory.Success(getProductList, "Successful extraction of products from whishList");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding product to whishList");
                return ResponseFactory.Error(new List<GetProductDto>(), "Internal error");
            }
        }

    }
}
