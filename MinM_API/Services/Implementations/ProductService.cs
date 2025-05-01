using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Products;
using MinM_API.Dtos.ProductVariant;
using MinM_API.Extension;
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
                    Slug = SlugExtension.GenerateSlug(addProductDto.Name),
                    Description = addProductDto.Description,
                    CategoryId = addProductDto.CategoryId,
                    SKU = addProductDto.SKU,
                    ProductImages = []
                };

                foreach(var productVariant in addProductDto.ProductVariants)
                {
                    product.ProductVariants.Add(new Models.ProductVariant()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = productVariant.Name,
                        Price = productVariant.Price,
                        UnitsInStock = productVariant.UnitsInStock,
                    });                
                }

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

        public async Task<ServiceResponse<int>> UpdateProduct(UpdateProductDto updateProductDto)
        {
            var serviceResponse = new ServiceResponse<int>();

            try
            {
                var product = await context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == updateProductDto.Id);

                if (product == null)
                {
                    serviceResponse.Message = "Product not found";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                product.Name = updateProductDto.Name;
                product.Slug = SlugExtension.GenerateSlug(updateProductDto.Name);
                product.Description = updateProductDto.Description;
                product.CategoryId = updateProductDto.CategoryId;
                product.SKU = updateProductDto.SKU;

                var existingImages = product.ProductImages.Select(pi => pi.FilePath).ToList();

                var newImages = updateProductDto.ImageUrls.Except(existingImages).ToList();

                var imagesToRemove = existingImages.Except(updateProductDto.ImageUrls).ToList();

                foreach (var imagePath in imagesToRemove)
                {
                    var imageToDelete = product.ProductImages.FirstOrDefault(pi => pi.FilePath == imagePath);
                    if (imageToDelete != null)
                    {
                        context.ProductImages.Remove(imageToDelete);
                        File.Delete(imagePath);
                    }
                }

                foreach (var newImage in newImages)
                {
                    product.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = product.Id,
                        FilePath = newImage
                    });
                }

                var existingVariants = product.ProductVariants.ToList();
                var dtoVariants = updateProductDto.ProductVariants;

                var dtoVariantIds = dtoVariants.Where(v => !string.IsNullOrEmpty(v.Id)).Select(v => v.Id).ToList();
                var variantsToRemove = existingVariants.Where(ev => !dtoVariantIds.Contains(ev.Id)).ToList();

                foreach (var variant in variantsToRemove)
                {
                    context.ProductVariants.Remove(variant);
                }

                foreach (var variantDto in dtoVariants)
                {
                    if (!string.IsNullOrEmpty(variantDto.Id))
                    {
                        var existing = existingVariants.FirstOrDefault(v => v.Id == variantDto.Id);
                        if (existing != null)
                        {
                            existing.Name = variantDto.Name;
                            existing.Price = variantDto.Price;
                            existing.UnitsInStock = variantDto.UnitsInStock;
                            existing.IsStock = variantDto.IsStock;
                        }
                    }
                    else
                    {
                        product.ProductVariants.Add(new ProductVariant
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = product.Id,
                            Name = variantDto.Name,
                            Price = variantDto.Price,
                            UnitsInStock = variantDto.UnitsInStock,
                            IsStock = variantDto.IsStock
                        });
                    }
                }

                await context.SaveChangesAsync();

                serviceResponse.Data = 1;
                serviceResponse.IsSuccessful = true;
                serviceResponse.Message = "Product successfully updated";
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

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProducts()
        {
            var serviceResponse = new ServiceResponse<List<GetProductDto>>();

            try
            {
                var productsList = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p => p.ProductImages)
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
            catch (Exception ex)
            {
                serviceResponse.Data = [];
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
                Slug = product.Slug,
                Description = product.Description,
                IsSeasonal = product.IsSeasonal,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SKU = product.SKU,
            };

            foreach (var image in product.ProductImages)
            {
                dto.ImageUrls.Add(new GetProductImageDto()
                {
                    FilePath = image.FilePath,
                });
            }

            foreach (var variant in product.ProductVariants)
            {
                dto.ProductVariants.Add(new GetProductVariantDto()
                {
                    Id = variant.Id,
                    Name = variant.Name,
                    Price = variant.Price,
                    DiscountPrice = variant.DiscountPrice,
                    UnitsInStock = variant.UnitsInStock,
                    IsStock = variant.IsStock,
                });
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

                if (product == null)
                {
                    serviceResponse.Data = new GetProductDto();
                    serviceResponse.IsSuccessful = false;
                    serviceResponse.Message = "There are no products";
                    serviceResponse.StatusCode = HttpStatusCode.NotFound;
                    return serviceResponse;
                }

                var getProduct = ConvertToDto(product);

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
