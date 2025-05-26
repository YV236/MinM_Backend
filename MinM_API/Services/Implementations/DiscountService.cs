using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
using MinM_API.Dtos.Products;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Linq;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class DiscountService(DataContext context, DiscountMapper mapper, ILogger<DiscountService> logger) : IDiscountService
    {
        public async Task<ServiceResponse<int>> AddDiscount(AddDiscountDto addDiscountDto)
        {
            try
            {
                var discount = new Discount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addDiscountDto.Name,
                    Slug = SlugExtension.GenerateSlug(addDiscountDto.Name),
                    DiscountPercentage = addDiscountDto.DiscountPercentage,
                    RemoveAfterExpiration = addDiscountDto.RemoveAfterExpiration,
                    StartDate = addDiscountDto.StartDate,
                    EndDate = addDiscountDto.EndDate,
                };

                var productList = await context.Products
                    .Where(p => addDiscountDto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (productList == null)
                {
                    logger.LogInformation("Fail: No products found in database");
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
                }

                foreach (var product in productList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.IsDiscounted = true;

                    foreach (var productVariant in product.ProductVariants)
                    {
                        productVariant.DiscountPrice = DiscountExtension.CountDiscountPrice(productVariant.Price, discount.DiscountPercentage);
                    }
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successful discount creation");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding discount. Name: {DiscountName}", addDiscountDto.Name);
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> UpdateDiscount(UpdateDiscountDto updateDiscountDto)
        {
            try
            {
                var discount = await context.Discounts
                    .Include(d => d.Products)
                    .FirstOrDefaultAsync(d => d.Id == updateDiscountDto.Id);

                if (discount == null)
                {
                    logger.LogInformation("Fail: No discounts found in database");
                    return ResponseFactory.Error(0, "Discount not found", HttpStatusCode.NotFound);
                }

                mapper.UpdateDiscountToDiscount(updateDiscountDto, discount);
                discount.Slug = SlugExtension.GenerateSlug(updateDiscountDto.Name);

                var discountedProductList = await context.Products
                    .Include(d => d.ProductVariants)
                    .Where(p => updateDiscountDto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (discountedProductList.Count == 0)
                {
                    logger.LogInformation("Fail: No products found in database");
                    return ResponseFactory.Error(0, "No products found for provided IDs", HttpStatusCode.NotFound);
                }

                discount.Products = discountedProductList;

                foreach (var product in discountedProductList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.IsDiscounted = true;
                    foreach (var productVariant in product.ProductVariants)
                    {
                        productVariant.DiscountPrice = DiscountExtension.CountDiscountPrice(productVariant.Price, discount.DiscountPercentage);
                    }
                }

                var previouslyDiscountedProducts = await context.Products
                    .Include(p => p.ProductVariants)
                    .Where(p => p.DiscountId == discount.Id)
                    .ToListAsync();

                foreach (var product in previouslyDiscountedProducts)
                {
                    if (!updateDiscountDto.ProductIds.Contains(product.Id))
                    {
                        product.Discount = null;
                        product.DiscountId = null;
                        product.IsDiscounted = false;
                        foreach (var variant in product.ProductVariants)
                        {
                            variant.DiscountPrice = 0;
                        }
                    }
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Discount successfully updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while updating discount. Name: {DiscountName}", updateDiscountDto.Name);
                return ResponseFactory.Error(0, "internal error");
            }
        }

        public async Task<ServiceResponse<List<GetDiscountDto>>> GetAllDiscounts()
        {
            try
            {
                var discountList = await context.Discounts.ToListAsync();

                if (discountList == null || discountList.Count == 0)
                {
                    logger.LogInformation("Fail: No discounts found in database");
                    return ResponseFactory.Error(new List<GetDiscountDto>(), "There are no discounts", HttpStatusCode.NotFound);
                }

                var getDiscountList = new List<GetDiscountDto>();

                foreach (var discount in discountList)
                {
                    getDiscountList.Add(mapper.DiscountToDiscountDto(discount));
                }

                return ResponseFactory.Success(getDiscountList, "Successful extraction of discounts");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving discounts from database");
                return ResponseFactory.Error(new List<GetDiscountDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetDiscountDto>> GetDiscountById(string id)
        {
            try
            {
                var discount = await context.Discounts.Include(d => d.Products)
                    .ThenInclude(p => p.ProductImages).FirstOrDefaultAsync(d => d.Id == id);

                if (discount == null)
                {
                    return ResponseFactory.Error(new GetDiscountDto(), "There is no discount with such id", HttpStatusCode.NotFound);
                }

                var getDiscountDto = mapper.DiscountToDiscountDto(discount);

                return ResponseFactory.Success(getDiscountDto, "Successful extraction of discount");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving discount from database with such id. Id: {Id}", id);
                return ResponseFactory.Error(new GetDiscountDto(), "Internal error");
            }
        }
    }
}
