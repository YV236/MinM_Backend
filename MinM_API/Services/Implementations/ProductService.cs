using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Products;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class ProductService(DataContext context) : IProductService
    {
        public async Task<ServiceResponse<string>> AddProduct(AddProductDto addProductDto)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {
                var product = new Models.Product()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addProductDto.Name,
                    Description = addProductDto.Description,
                    Price = addProductDto.Price,
                    ProductVariant = addProductDto.ProductVariant,
                    UnitsInStock = addProductDto.UnitsInStock,
                    IsStock = addProductDto.IsStock,
                    CategoryId = addProductDto.CategoryId,
                    SKU = addProductDto.SKU,
                    ProductImages = []
                };

                if (addProductDto.ImageUrls != null)
                {
                    foreach (var image in addProductDto.ImageUrls)
                    {
                        product.ProductImages.Add(new Models.ProductImage()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = product.Id,
                            FilePath = image
                        });
                    }
                }

                context.Products.Add(product);
                await context.SaveChangesAsync();

                serviceResponse.Data = product.Id;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Product successfully added";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                serviceResponse.Data = null;
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProducts()
        {
            var serviceResponse = new ServiceResponse<List<GetProductDto>>();

            try
            {
                var productsList = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p=>p.ProductImages)
                    .ToListAsync();

                if (productsList == null || productsList.Count == 0)
                {
                    serviceResponse.Data = [];
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There are no products";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                var getProductsList = new List<GetProductDto>();

                foreach (var product in productsList)
                {
                    getProductsList.Add(ConvertToDto(product));
                }

                serviceResponse.Data = getProductsList;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful extraction of products";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch(Exception ex)
            {
                serviceResponse.Data = new List<GetProductDto>();
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }

        private static GetProductDto ConvertToDto(Product product)
        {
            var dto = new GetProductDto()
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

            foreach(var image in product.ProductImages)
            {
                dto.ImageUrls.Add(new GetProductImageDto()
                {
                    FilePath = image.FilePath,
                });
            }

            foreach (var variant in product.ProductVariant)
            {
                dto.ProductVariant.Add(variant);
            }

            return dto;
        }

        public async Task<ServiceResponse<GetProductDto>> GetProductById(string id)
        {
            var serviceResponse = new ServiceResponse<GetProductDto>();

            try
            {
                var product = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if(product == null) 
                {
                    serviceResponse.Data = new GetProductDto();
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There are no products";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                var getProduct=ConvertToDto(product);

                serviceResponse.Data = getProduct;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Successful extraction of product by id";
                serviceResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {

                serviceResponse.Data = new GetProductDto();
                serviceResponse.IsSuccessful = false;
                serviceResponse.Message = ex.Message;
                serviceResponse.StatusCode = HttpStatusCode.BadRequest;
            }

            return serviceResponse;
        }
    }
}
