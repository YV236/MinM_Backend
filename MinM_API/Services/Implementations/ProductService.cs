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
                return ResponseFactory.Error(0, "No Category with specified id found", HttpStatusCode.NotFound);
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

                mapper.UpdateProductToProduct(updateProductDto, product);
                product.Slug = SlugExtension.GenerateSlug(product.Name);

                UpdateProductImages(product, updateProductDto.ImageUrls);

                var discount = await context.Discounts.FirstOrDefaultAsync(d => d.Id == product.DiscountId);
                UpdateProductVariants(product, updateProductDto.ProductVariants, discount);

                await context.SaveChangesAsync();
                return ResponseFactory.Success(1, "Product successfully updated");
            }
            catch (Exception)
            {
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

        private void UpdateProductImages(Product product, List<string> newImageUrls)
        {
            var existingImages = product.ProductImages.Select(pi => pi.FilePath).ToList();

            var toAdd = newImageUrls.Except(existingImages).ToList();
            var toRemove = existingImages.Except(newImageUrls).ToList();

            foreach (var imagePath in toRemove)
            {
                var img = product.ProductImages.FirstOrDefault(pi => pi.FilePath == imagePath);
                if (img != null)
                {
                    context.ProductImages.Remove(img);
                    File.Delete(imagePath);
                }
            }

            foreach (var path in toAdd)
            {
                product.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    FilePath = path
                });
            }
        }

        private void UpdateProductVariants(Product product, List<UpdateProductVariantDto> variantsDto, Discount? discount)
        {
            var existingVariants = product.ProductVariants.ToList();
            var dtoIds = variantsDto.Where(v => !string.IsNullOrEmpty(v.Id)).Select(v => v.Id).ToList();

            var toRemove = existingVariants.Where(ev => !dtoIds.Contains(ev.Id)).ToList();
            foreach (var v in toRemove)
            {
                context.ProductVariants.Remove(v);
            }

            foreach (var variantDto in variantsDto)
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
        }
    }
}
