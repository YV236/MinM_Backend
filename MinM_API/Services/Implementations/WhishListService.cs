using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProductsFromWishList(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                if (userId.IsNullOrEmpty())
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(new List<GetProductDto>(), "There is no such user with such id");
                }

                var userProducts = await context.WishlistItems
                    .Where(wi => wi.UserId == userId)
                    .Select(wi => wi.Product)
                    .ToListAsync();

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

        public async Task<ServiceResponse<GetProductDto>> GetProductFromWishList(ClaimsPrincipal user, string whishListItemId)
        {
            try
            {
                var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                var wishListItem = await context.WishlistItems
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Discount)
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Season)
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.ProductImages.OrderBy(pi => pi.SequenceNumber))
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Colors)
                    .FirstOrDefaultAsync(wi => wi.ProductId == whishListItemId
                    && wi.UserId == userId);

                var product = wishListItem?.Product;

                if (product == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no product with such id: {whishListItemId}", whishListItemId);
                    return ResponseFactory.Error(new GetProductDto(), "There is no such product with such id");
                }

                var getProduct = mapper.ProductToGetProductDto(product);

                return ResponseFactory.Success(getProduct, "Successful extraction of product by id");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving product from database with such id. Id: {whishListItemId}", whishListItemId);
                return ResponseFactory.Error(new GetProductDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> ActualizeUserWishList(ClaimsPrincipal user, List<string> productIds)
        {
            try
            {
                var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                var currentProductIds = await context.WishlistItems
                    .Where(wi => wi.UserId == userId)
                    .Select(wi => wi.ProductId)
                    .ToListAsync();

                var productIdsToAdd = productIds.Except(currentProductIds).ToList();

                var newWishlistItems = productIdsToAdd.Select(productId => new WishlistItem
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                });

                context.WishlistItems.AddRange(newWishlistItems);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successful whishList update");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating whishList");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteProductFromWishList(ClaimsPrincipal user, string whishListItemId)
        {
            try
            {
                var userId = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                var wishListItem = await context.WishlistItems
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Discount)
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Season)
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.ProductImages.OrderBy(pi => pi.SequenceNumber))
                    .Include(wi => wi.Product)
                    .ThenInclude(p => p.Colors)
                    .FirstOrDefaultAsync(wi => wi.ProductId == whishListItemId
                    && wi.UserId == userId);

                if(wishListItem is null)
                {
                    logger.LogError("Fail: Fetching error. There's no item with such id {itemId}", whishListItemId);
                    return ResponseFactory.Error(0, "Internal error");
                }

                context.WishlistItems.Remove(wishListItem);
                context.SaveChanges();

                return ResponseFactory.Success(1, "Successful removal of the product from whishList");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error while deleting product from whishList");
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
