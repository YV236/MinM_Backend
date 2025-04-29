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
            var serviceResponse = new ServiceResponse<int>();

            try
            {
                var discount = new Models.Discount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Slug = SlugExtension.GenerateSlug(dto.Name),
                    DiscountPercentage = dto.DiscountPercentage,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                };

                var productList = await context.Products
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (productList == null)
                {
                    serviceResponse.Data = 0;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "Product not found";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                foreach (var product in productList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.DiscountPrice = CountDiscountPrice(product.Price, discount.DiscountPercentage);
                }

                await context.SaveChangesAsync();

                serviceResponse.Data = 1;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful discount creation";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
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
            var serviceResponse = new ServiceResponse<int>();

            try
            {
                var discount = await context.Discounts
                    .Include(d => d.Products)
                    .FirstOrDefaultAsync(d => d.Id == dto.Id);

                if (discount == null)
                {
                    serviceResponse.Message = "Discount not found";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                discount.Name = dto.Name;
                discount.Slug = SlugExtension.GenerateSlug(dto.Name);
                discount.DiscountPercentage = dto.DiscountPercentage;
                discount.StartDate = dto.StartDate;
                discount.EndDate = dto.EndDate;

                var updatedProductIds = dto.ProductIds.ToHashSet();
                foreach (var oldProduct in discount.Products.ToList())
                {
                    if (!updatedProductIds.Contains(oldProduct.Id))
                    {
                        oldProduct.Discount = null;
                        oldProduct.DiscountId = null;
                        oldProduct.DiscountPrice = null;
                        oldProduct.IsDiscounted = false;
                    }
                }

                var productList = await context.Products
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();

                if (productList.Count == 0)
                {
                    serviceResponse.Data = 0;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "No products found for provided IDs";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                discount.Products = productList;

                foreach (var product in productList)
                {
                    product.Discount = discount;
                    product.DiscountId = discount.Id;
                    product.DiscountPrice = CountDiscountPrice(product.Price, discount.DiscountPercentage);
                    product.IsDiscounted = true;
                }

                await context.SaveChangesAsync();

                serviceResponse.Data = 1;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Discount successfully updated";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = 0;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }


        public async Task<ServiceResponse<List<GetDiscountDto>>> GetAllDiscounts()
        {
            var serviceResponse = new ServiceResponse<List<GetDiscountDto>>();

            try
            {
                var discountList = await context.Discounts.ToListAsync();

                if (discountList == null || discountList.Count == 0)
                {
                    serviceResponse.Data = [];
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There are no discounts";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
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

                serviceResponse.Data = getDiscountList;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful extraction of discounts";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = [];
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetDiscountDto>> GetDiscountById(string id)
        {
            var serviceResponse = new ServiceResponse<GetDiscountDto>();

            try
            {
                var discount = await context.Discounts.Include(d => d.Products)
                    .ThenInclude(p => p.ProductImages).FirstOrDefaultAsync(d => d.Id == id);

                if (discount == null)
                {
                    serviceResponse.Data = null;
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There is no discount with such id";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
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
                        Price = product.Price,
                        UnitsInStock = product.UnitsInStock,
                        IsStock = product.IsStock,
                        IsSeasonal = product.IsSeasonal,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category.Name,
                        SKU = product.SKU,
                    };

                    if (product.Discount != null)
                        dto.DiscountPrice = dto.Price * (product.DiscountPrice / 100);

                    foreach (var image in product.ProductImages)
                    {
                        dto.ImageUrls.Add(new GetProductImageDto()
                        {
                            FilePath = image.FilePath,
                        });
                    }

                    getDiscount.Products.Add(dto);
                }

                serviceResponse.Data = getDiscount;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful extraction of discount";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = new GetDiscountDto();
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }
    }
}
