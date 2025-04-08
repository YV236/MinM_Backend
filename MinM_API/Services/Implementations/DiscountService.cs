using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Discount;
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
                    DiscountPercentage = dto.DiscountPercentage,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                };

                var productIds = dto.Products.Select(p => p.Id).ToList();

                var productList = await context.Products
                    .Where(p => productIds.Contains(p.Id))
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
    }
}
