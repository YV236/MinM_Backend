using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Products;
using MinM_API.Dtos.ProductVariant;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class ProductService(DataContext context, ProductMapper mapper, ILogger<ProductService> logger) : IProductService
    {
        public async Task<ServiceResponse<string>> AddProduct(AddProductDto addProductDto)
        {
            try
            {
                var product = new Product()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addProductDto.Name,
                    Slug = SlugExtension.GenerateSlug(addProductDto.Name),
                    Description = addProductDto.Description,
                    CategoryId = addProductDto.CategoryId,
                    SKU = addProductDto.SKU,
                    ProductImages = []
                };

                foreach (var productVariant in addProductDto.ProductVariants)
                {
                    product.ProductVariants.Add(new ProductVariant()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = productVariant.Name,
                        Price = productVariant.Price,
                        UnitsInStock = productVariant.UnitsInStock,
                        IsStock = productVariant.IsStock,
                    });
                }

                if (addProductDto.ImageUrls != null)
                {
                    foreach (var image in addProductDto.ImageUrls)
                    {
                        product.ProductImages.Add(new ProductImage()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = product.Id,
                            FilePath = image
                        });
                    }
                }

                context.Products.Add(product);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(product.Id, "Product successfully added");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while adding product. Name: {ProductName}, CategoryId: {CategoryId}",
                    addProductDto.Name, addProductDto.CategoryId);
                return ResponseFactory.Error("", "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> UpdateProduct(UpdateProductDto updateProductDto)
        {
            if (!await context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId))
            {
                return ResponseFactory.Error(0, "No Category with specified id where found", HttpStatusCode.NotFound);
            }

            try
            {
                var product = await context.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.Id == updateProductDto.Id);

                if (product == null)
                {
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
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

                var discount = await context.Discounts.FirstOrDefaultAsync(d => d.Id == product.DiscountId);

                foreach (var variantDto in dtoVariants)
                {
                    if (!string.IsNullOrEmpty(variantDto.Id))
                    {
                        var existing = existingVariants.FirstOrDefault(v => v.Id == variantDto.Id);
                        if (existing != null)
                        {
                            existing.Name = variantDto.Name;
                            existing.Price = variantDto.Price;
                            existing.DiscountPrice = discount != null
                                ? DiscountExtension.CountDiscountPrice(variantDto.Price, discount.DiscountPercentage) : 0;

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
                            DiscountPrice = discount != null
                            ? DiscountExtension.CountDiscountPrice(variantDto.Price, discount.DiscountPercentage) : 0,
                            UnitsInStock = variantDto.UnitsInStock,
                            IsStock = variantDto.IsStock
                        });
                    }
                }

                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Product successfully updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while updating product. Name: {ProductName}, CategoryId: {CategoryId}",
                    updateProductDto.Name, updateProductDto.CategoryId);
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<GetProductDto>>> GetAllProducts()
        {
            try
            {
                var productsList = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p => p.ProductImages)
                    .ToListAsync();

                if (productsList == null || productsList.Count == 0)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new List<GetProductDto>(), "There are no products", HttpStatusCode.NotFound);
                }

                var getProductsList = new List<GetProductDto>();

                foreach (var product in productsList)
                {
                    getProductsList.Add(mapper.ProductToGetProductDto(product));
                }

                logger.LogInformation("Retrieved {Count} products from database", getProductsList.Count);

                return ResponseFactory.Success(getProductsList, "Successful extraction of products");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while to retrieving products from database");
                return ResponseFactory.Error(new List<GetProductDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetProductDto>> GetProductById(string id)
        {
            try
            {
                var product = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new GetProductDto(), "There are no products", HttpStatusCode.NotFound);
                }

                var getProduct = mapper.ProductToGetProductDto(product);

                return ResponseFactory.Success(getProduct, "Successful extraction of product by id");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving product from database with such id. Id: {Id}", id);
                return ResponseFactory.Error(new GetProductDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<GetProductDto>> GetProductBySlug(string slug)
        {
            try
            {
                var product = await context.Products
                    .Include(p => p.Discount)
                    .Include(p => p.Season)
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Slug == slug);

                if (product == null)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new GetProductDto(), "There are no products", HttpStatusCode.NotFound);
                }

                var getProduct = mapper.ProductToGetProductDto(product);

                return ResponseFactory.Success(getProduct, "Successful extraction of product by slug");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while retrieving product from database with such slug. Slug: {slug}", slug);
                return ResponseFactory.Error(new GetProductDto(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> DeleteProduct(string id)
        {
            try
            {
                var productToDelete = await context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (productToDelete == null)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
                }

                context.Products.Remove(productToDelete);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(1, "Product successfully removed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while deleting {Id} from database", id);
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
