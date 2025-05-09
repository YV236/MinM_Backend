using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
using MinM_API.Dtos.Products;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Linq;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class DiscountService(DataContext context) : IDiscountService
    {
        public async Task<ServiceResponse<int>> AddDiscount(AddDiscountDto dto)
        {
            try
            {
                var discount = new Discount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Slug = SlugExtension.GenerateSlug(dto.Name),
                    DiscountPercentage = dto.DiscountPercentage,
                    RemoveAfterExpiration = dto.RemoveAfterExpiration,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                };

                var productList = await context.Products
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (productList == null)
                {
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
                }

                foreach (var product in productList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.IsDiscounted = true;

                    foreach (var productVariant in product.ProductVariants)
                    {
                        productVariant.DiscountPrice = CountDiscountPrice(productVariant.Price, discount.DiscountPercentage);
                    }
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Successful discount creation");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        private static decimal CountDiscountPrice(decimal price, decimal discountPercentage)
        {
            var whole = Math.Floor(price);

            var fractional = price - whole;

            decimal discountedPrice = whole - (whole * (discountPercentage / 100));

            discountedPrice = Math.Floor(discountedPrice);

            return discountedPrice + fractional;
        }

        public async Task<ServiceResponse<int>> UpdateDiscount(UpdateDiscountDto dto)
        {
            try
            {
                var discount = await context.Discounts
                    .Include(d => d.Products)
                    .FirstOrDefaultAsync(d => d.Id == dto.Id);

                if (discount == null)
                {
                    return ResponseFactory.Error(0, "Discount not found", HttpStatusCode.NotFound);
                }

                discount.Name = dto.Name;
                discount.Slug = SlugExtension.GenerateSlug(dto.Name);
                discount.DiscountPercentage = dto.DiscountPercentage;
                discount.StartDate = dto.StartDate;
                discount.EndDate = dto.EndDate;
                discount.RemoveAfterExpiration = dto.RemoveAfterExpiration;
                discount.IsActive = true;

                var updatedProductIds = dto.ProductIds.ToHashSet();
                foreach (var oldProduct in discount.Products.ToList())
                {
                    if (!updatedProductIds.Contains(oldProduct.Id))
                    {
                        oldProduct.Discount = null;
                        oldProduct.DiscountId = null;
                        oldProduct.IsDiscounted = false;

                        foreach (var productVariant in oldProduct.ProductVariants)
                        {
                            productVariant.DiscountPrice = null;
                        }
                    }
                }

                var productList = await context.Products
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (productList.Count == 0)
                {
                    return ResponseFactory.Error(0, "No products found for provided IDs", HttpStatusCode.NotFound);
                }

                discount.Products = productList;

                foreach (var product in productList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.IsDiscounted = true;
                    foreach (var productVariant in product.ProductVariants)
                    {
                        productVariant.DiscountPrice = CountDiscountPrice(productVariant.Price, discount.DiscountPercentage);
                    }
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Discount successfully updated");
            }
            catch (Exception ex)
            {
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
                    return ResponseFactory.Error(new List<GetDiscountDto>(), "There are no discounts", HttpStatusCode.NotFound);
                }

                var getDiscountList = new List<GetDiscountDto>();

                foreach (var discount in discountList)
                {
                    getDiscountList.Add(new GetDiscountDto
                    {
                        Id = discount.Id,
                        Name = discount.Name,
                        Slug = discount.Slug,
                        DiscountPercentage = discount.DiscountPercentage,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                    });
                }

                return ResponseFactory.Success(getDiscountList, "Successful extraction of discounts");
            }
            catch (Exception ex)
            {
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

                var getDiscount = new GetDiscountDto
                {
                    Id = id,
                    Name = discount.Name,
                    Slug = discount.Slug,
                    DiscountPercentage = discount.DiscountPercentage,
                    StartDate = discount.StartDate,
                    EndDate = discount.EndDate,
                };

                foreach (var product in discount.Products)
                {
                    var dto = new GetProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        IsSeasonal = product.IsSeasonal,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category.Name,
                        SKU = product.SKU,
                    };

                    foreach (var productVariant in product.ProductVariants)
                    {
                        dto.ProductVariants.Add(new Dtos.ProductVariant.GetProductVariantDto()
                        {
                            Id = productVariant.Id,
                            Name = productVariant.Name,
                            Price = productVariant.Price,
                            DiscountPrice = productVariant.DiscountPrice,
                            UnitsInStock = productVariant.UnitsInStock,
                            IsStock = productVariant.IsStock
                        });
                    }

                    foreach (var image in product.ProductImages)
                    {
                        dto.ImageUrls.Add(new GetProductImageDto()
                        {
                            FilePath = image.FilePath,
                        });
                    }

                    getDiscount.Products.Add(dto);
                }

                return ResponseFactory.Success(getDiscount, "Successful extraction of discount");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error(new GetDiscountDto(), "Internal error");
            }
        }
    }
}
