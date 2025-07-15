using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using MinM_API.Dtos.ProductVariant;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class CartService(DataContext context, ILogger<CartService> logger, ProductMapper mapper) : ICartService
    {
        public async Task<ServiceResponse<int>> ActualizeUserCart(ClaimsPrincipal user, List<string> productIds)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var userEntity = await context.Users
                    .Include(u => u.Cart)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (userEntity == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var products = await context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var product in products)
                { 
                    userEntity.Cart.Add(product); 
                }

                await context.SaveChangesAsync();
                
                return ResponseFactory.Success(0, "Successful whishList update");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating whishList");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> AddProductToCart(ClaimsPrincipal user, string productId)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var userEntity = await context.Users
                    .Include(u => u.Cart)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (userEntity == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var product = await context.Products
                    .SingleOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    logger.LogInformation("Fail: Adding error. There's no product with such id: {productId}", productId);
                    return ResponseFactory.Error(0, "There is no product with such id");
                }

                if (!userEntity.Cart.Any(p => p.Id == productId))
                {
                    userEntity.Cart.Add(product);
                    await context.SaveChangesAsync();
                }
                else
                {
                    logger.LogInformation("Info: Product already in cart. UserId: {userId}, ProductId: {productId}", userId, productId);
                }

                return ResponseFactory.Success(userEntity.Cart.Count, "Product successfully added to cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while adding product to cart");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteProductFromCart(ClaimsPrincipal user, string cartItemId)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no user with such id");
                }

                var userEntity = await context.Users
                    .Include(u => u.Cart)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (userEntity == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no user with such id");
                }

                var productToRemove = userEntity.Cart.SingleOrDefault(p => p.Id == cartItemId);
                if (productToRemove == null)
                {
                    logger.LogInformation("Fail: Removing error. Product not found in cart. UserId: {userId}, ProductId: {cartItemId}", userId, cartItemId);
                    return ResponseFactory.Error(userEntity.Cart.Count, "Product not found in user cart");
                }

                userEntity.Cart.Remove(productToRemove);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(userEntity.Cart.Count, "Product successfully removed from cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while removing product from cart");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProductsFromCart(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error<List<GetProductDto>>(null, "There is no such user with such id");
                }

                var userEntity = await context.Users
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.ProductVariants)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Discount)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Category)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.ProductImages)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Colors)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (userEntity == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error<List<GetProductDto>>(null, "There is no such user with such id");
                }

                var productDtos = userEntity.Cart.Select(product => mapper.ProductToGetProductDto(product)).ToList();

                return ResponseFactory.Success(productDtos, "Successfully fetched products from cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching products from cart");
                return ResponseFactory.Error<List<GetProductDto>>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<GetProductDto>> GetProductFromCart(ClaimsPrincipal user, string cartItemId)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error<GetProductDto>(null, "There is no such user with such id");
                }

                var userEntity = await context.Users
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.ProductVariants)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Discount)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Category)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.ProductImages)
                    .Include(u => u.Cart)
                        .ThenInclude(p => p.Colors)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (userEntity == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error<GetProductDto>(null, "There is no such user with such id");
                }

                var product = userEntity.Cart.SingleOrDefault(p => p.Id == cartItemId);
                if (product == null)
                {
                    logger.LogInformation("Fail: Product not found in cart. UserId: {userId}, ProductId: {cartItemId}", userId, cartItemId);
                    return ResponseFactory.Error<GetProductDto>(null, "Product not found in user cart");
                }

                var dto = mapper.ProductToGetProductDto(product);

                return ResponseFactory.Success(dto, "Successfully fetched product from cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching product from cart");
                return ResponseFactory.Error<GetProductDto>(null, "Internal error");
            }
        }
    }
}
