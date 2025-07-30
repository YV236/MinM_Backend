using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Product;
using MinM_API.Dtos.ProductVariant;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace MinM_API.Services.Implementations
{
    public class ProductService(DataContext context, ProductMapper mapper,
        ILogger<ProductService> logger, IPhotoService photoService) : IProductService
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<ServiceResponse<string>> AddProduct(AddProductDto addProductDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(addProductDto.Name))
                {
                    return ResponseFactory.Error("", "Product name is required", HttpStatusCode.BadRequest);
                }

                var product = new Product()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addProductDto.Name,
                    Slug = SlugExtension.GenerateSlug(addProductDto.Name),
                    Description = addProductDto.Description,
                    CategoryId = addProductDto.CategoryId,
                    SKU = addProductDto.SKU,
                    DateOfCreation = DateTime.UtcNow,
                    IsNew = true,
                    ProductImages = []
                };

                if (context.Products.Any(p => p.Slug == product.Slug))
                {
                    return ResponseFactory.Error("", $"You already have a product with this name '{product.Name}'", HttpStatusCode.BadRequest);
                }

                if (addProductDto.ProductVariantsJson.IsNullOrEmpty())
                {
                    return ResponseFactory.Error("", "Product must have at least one variant", HttpStatusCode.BadRequest);
                }
                
                var variants = JsonSerializer.Deserialize<List<AddProductVariantDto>>(addProductDto.ProductVariantsJson, jsonOptions);                

                foreach (var productVariant in variants)
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

                if (addProductDto.Images?.Count != 0 && addProductDto.ImageSequenceNumbers?.Count != 0)
                {
                    if (addProductDto.Images.Count != addProductDto.ImageSequenceNumbers.Count)
                    {
                        return ResponseFactory.Error("", "Number of images must match number of sequence numbers", HttpStatusCode.BadRequest);
                    }

                    for (int i = 0; i < addProductDto.Images.Count; i++)
                    {
                        var image = addProductDto.Images[i];
                        var sequenceNumber = addProductDto.ImageSequenceNumbers[i];

                        product.ProductImages.Add(new ProductImage()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = product.Id,
                            SequenceNumber = sequenceNumber,
                            FilePath = await photoService.UploadImageAsync(image)
                        });
                    }
                }

                if (!addProductDto.ProductColorsJson.IsNullOrEmpty())
                {
                    var colors = JsonSerializer.Deserialize<List<ColorDto>>(addProductDto.ProductColorsJson, jsonOptions);

                    foreach (var color in colors)
                    {
                        product.Colors.Add(await AddProductColor(color));
                    }
                }

                context.Products.Add(product);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(product.Id, "Product successfully added");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON deserialization error while adding product. Name: {ProductName}", addProductDto.Name);
                return ResponseFactory.Error("", "Invalid JSON format in request data", HttpStatusCode.BadRequest);
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
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.Id == updateProductDto.Id);

                if (product == null)
                {
                    return ResponseFactory.Error(0, "Product not found", HttpStatusCode.NotFound);
                }

                mapper.UpdateProductToProduct(updateProductDto, product);
                product.Slug = SlugExtension.GenerateSlug(product.Name);

                await UpdateProductImagesAsync(updateProductDto);

                var discount = await context.Discounts.FirstOrDefaultAsync(d => d.Id == product.DiscountId);

                var variants = JsonSerializer.Deserialize<List<UpdateProductVariantDto>>(updateProductDto.ProductVariantsJson, jsonOptions);

                UpdateProductVariants(product, variants, discount);

                if (!updateProductDto.ProductColorsJson.IsNullOrEmpty())
                {
                    var colors = JsonSerializer.Deserialize<List<ColorDto>>(updateProductDto.ProductColorsJson, jsonOptions);
                    await UpdateProductColors(product, colors);
                }

                await context.SaveChangesAsync();
                return ResponseFactory.Success(1, "Product successfully updated");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON deserialization error while adding product. Name: {ProductName}", updateProductDto.Name);
                return ResponseFactory.Error(0, "Invalid JSON format in request data", HttpStatusCode.BadRequest);
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
                    .Include(p => p.ProductVariants)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Colors)
                    .ToListAsync();

                if (productsList == null || productsList.Count == 0)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new List<GetProductDto>(), "There are no products", HttpStatusCode.NotFound);
                }

                foreach (var product in productsList)
                {
                    product.ProductVariants = product.ProductVariants
                        .OrderBy(pv => int.Parse(pv.Name))
                        .ToList();

                    product.ProductImages = product.ProductImages
                        .OrderBy(pi => pi.SequenceNumber)
                        .ToList();
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
                    .Include(p => p.ProductVariants)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Colors)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new GetProductDto(), "There are no products", HttpStatusCode.NotFound);
                }
                
                product.ProductVariants = product.ProductVariants
                    .OrderBy(pv => int.Parse(pv.Name))
                    .ToList();
                
                product.ProductImages = product.ProductImages
                    .OrderBy(pi => pi.SequenceNumber)
                    .ToList();

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
                    .Include(p => p.ProductVariants)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Colors)
                    .FirstOrDefaultAsync(p => p.Slug == slug);

                if (product == null)
                {
                    logger.LogInformation("No products found in database");
                    return ResponseFactory.Error(new GetProductDto(), "There are no products", HttpStatusCode.NotFound);
                }

                product.ProductVariants = product.ProductVariants
                   .OrderBy(pv => int.Parse(pv.Name))
                   .ToList();

                product.ProductImages = product.ProductImages
                    .OrderBy(pi => pi.SequenceNumber)
                    .ToList();

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

                foreach (var image in productToDelete.ProductImages)
                {
                    var publicId = photoService.GetPublicIdFromUrl(image.FilePath);
                    await photoService.DeleteImageAsync(publicId);
                    context.ProductImages.Remove(image);
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

        private async Task UpdateProductImagesAsync(UpdateProductDto updateProductDto)
        {
            var images = updateProductDto.Images;
            var sequenceNumbers = updateProductDto.ImageSequenceNumbers;
            if (images.Count != sequenceNumbers.Count)
                throw new ArgumentException("Images count і SequenceNumbers count must be equal.");

            // 1. Завантажуємо та видаляємо повністю всі старі фото
            var existingImages = await context.ProductImages
                .Where(pi => pi.ProductId == updateProductDto.Id)
                .ToListAsync();

            foreach (var oldImage in existingImages)
            {
                var publicId = photoService.GetPublicIdFromUrl(oldImage.FilePath);
                if (!string.IsNullOrEmpty(publicId))
                    await photoService.DeleteImageAsync(publicId);
                context.ProductImages.Remove(oldImage);
            }
            // Вже повний "очищений" список — можна додавати заново в потрібному порядку

            // 2. Додаємо фото згідно нового списку
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var sequenceNumber = sequenceNumbers[i];
                string? filePath = null;
                
                filePath = await photoService.UploadImageAsync(image);

                if (!string.IsNullOrEmpty(filePath))
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = updateProductDto.Id,
                        SequenceNumber = sequenceNumber,
                        FilePath = filePath
                    });
                }
            }

            await context.SaveChangesAsync();
        }


        private void UpdateProductVariants(Product product, List<UpdateProductVariantDto>? variants, Discount? discount)
        {
            var existingVariants = product.ProductVariants.ToList();
            var dtoIds = variants.Where(v => !string.IsNullOrEmpty(v.Id)).Select(v => v.Id).ToList();

            var toRemove = existingVariants.Where(ev => !dtoIds.Contains(ev.Id)).ToList();
            foreach (var v in toRemove)
            {
                context.ProductVariants.Remove(v);
            }

            foreach (var variantDto in variants)
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

        private async Task<Color> AddProductColor(ColorDto colorDto)
        {
            var color = await context.Colors.FirstOrDefaultAsync(c => c.ColorHex == colorDto.ColorHex);

            if (color is null)
            {
                return new Color() { Id = Guid.NewGuid().ToString(), Name = colorDto.Name, ColorHex = colorDto.ColorHex };
            }
            else
            {
                return color;
            }
        }

        private async Task UpdateProductColors(Product product, List<ColorDto> ColorsDto)
        {
            var colorHexes = ColorsDto.Select(c => c.ColorHex).ToList();

            var existingColors = await context.Colors
                .Where(c => colorHexes.Contains(c.ColorHex))
                .ToListAsync();

            var existingHexCodes = existingColors.Select(c => c.ColorHex).ToHashSet();

            var missingColorDtos = ColorsDto
                .Where(dto => !existingHexCodes.Contains(dto.ColorHex))
                .ToList();

            var newColors = new List<Color>();

            foreach (var colorDto in missingColorDtos)
            {
                var newColor = new Color
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = colorDto.Name,
                    ColorHex = colorDto.ColorHex
                };
                newColors.Add(newColor);
                context.Colors.Add(newColor);
            }

            if (newColors.Any())
            {
                await context.SaveChangesAsync();
            }

            var allColors = existingColors.Concat(newColors).ToList();

            product.Colors.Clear();
            foreach (var color in allColors)
            {
                product.Colors.Add(color);
            }
        }
    }
}
