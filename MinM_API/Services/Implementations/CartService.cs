using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Cart;
using MinM_API.Dtos.Product;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class CartService(DataContext context, ILogger<CartService> logger, ProductMapper productMapper, 
        CartMapper cartMapper) : ICartService
    {
        public async Task<ServiceResponse<int>> ActualizeUserCart(ClaimsPrincipal user, List<AddCartItemDto> items)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var userCart = await context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var cartDict = userCart.ToDictionary(
                    c => (c.ProductId, c.ProductVariantId),
                    c => c
                );

                foreach (var dto in items)
                {
                    if (dto.Quantity <= 0)
                        continue;

                    if (cartDict.TryGetValue((dto.ProductId, dto.ProductVariantId), out var existingCartItem))
                    {
                        existingCartItem.Quantity = dto.Quantity;
                    }
                    else
                    {
                        var newCartItem = new CartItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            ProductId = dto.ProductId,
                            ProductVariantId = dto.ProductVariantId,
                            Quantity = dto.Quantity,
                            AddedAt = DateTime.UtcNow
                        };
                        context.CartItems.Add(newCartItem);
                    }
                }

                await context.SaveChangesAsync();

                var totalCount = await context.CartItems.CountAsync(c => c.UserId == userId);
                return ResponseFactory.Success(totalCount, "Successfully updated cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating cart");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> AddProductToCart(ClaimsPrincipal user, AddCartItemDto cartItemDto)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    ProductId = cartItemDto.ProductId,
                    ProductVariantId = cartItemDto.ProductVariantId,
                    Quantity = cartItemDto.Quantity,
                    AddedAt = DateTime.UtcNow,
                };

                context.CartItems.Add(cartItem);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Product successfully added to cart");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding product to cart");
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
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var cartItem = await context.CartItems.FindAsync(cartItemId);

                if (cartItem == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no product in cart");
                    return ResponseFactory.Error(0, "There is no such product in cart");
                }

                context.CartItems.Remove(cartItem);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Product successfully added to cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding product to cart");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteProductFromCart(ClaimsPrincipal user, List<string> cartItemIds)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                return ResponseFactory.Error(0, "There is no such user with such id");
            }

            var cartItems = await context.CartItems
                .Include(c => c.Product)
                .Include(c => c.ProductVariant)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if(cartItems.Count == 0)
            {
                logger.LogInformation("Fail: there is no products in users cart");
                return ResponseFactory.Error(0, "There's now products in users cart", System.Net.HttpStatusCode.NotFound);
            }

            var cartItemsToRemove = cartItems.Where(ci => cartItemIds.Contains(ci.Id)).ToList();

            context.CartItems.RemoveRange(cartItemsToRemove);
            await context.SaveChangesAsync();

            return ResponseFactory.Success(1, "Products from cart removed successfully");
        }

        public async Task<ServiceResponse<List<GetCartItemDto>>> GetAllProductsFromCart(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(new List<GetCartItemDto>(), "There is no such user with such id");
                }

                var cartItems = await context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductVariant)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var getCartItems = new List<GetCartItemDto>();

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Product == null || cartItem.ProductVariant == null)
                        continue;

                    getCartItems.Add(cartMapper.CarItemToGetCartItemDto(cartItem));
                }

                return ResponseFactory.Success(getCartItems, "Successfully fetched products from cart");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while fetching cart products");
                return ResponseFactory.Error(new List<GetCartItemDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> UpdateCartItem(ClaimsPrincipal user, UpdateCartItemDto cartItemToUpdate)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    logger.LogInformation("Fail: Fetching error. There's no user with such id: {userId}", userId);
                    return ResponseFactory.Error(0, "There is no such user with such id");
                }

                var cartItem = await context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductVariant)
                    .FirstOrDefaultAsync(c => c.Id == cartItemToUpdate.Id);

                cartItem.ProductVariantId = cartItemToUpdate.ProductVariantId;
                cartItem.Quantity = cartItemToUpdate.Quantity;

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successfully updated cart item");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Fail: Error while updating cart item");
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
