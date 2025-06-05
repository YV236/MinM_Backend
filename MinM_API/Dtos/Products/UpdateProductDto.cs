using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Products
{
    public class UpdateProductDto
    {
        [FromForm] public string Id { get; set; } = string.Empty;
        [FromForm] public string Name { get; set; } = string.Empty;
        [FromForm] public string Description { get; set; } = string.Empty;
        [FromForm(Name = "ProductVariantsJson")] public string ProductVariantsJson { get; set; } = string.Empty;
        [FromForm] public string CategoryId { get; set; } = string.Empty;
        [FromForm] public string SKU { get; set; } = string.Empty;
        [FromForm] public List<string> ExistingImageUrls { get; set; } = new();
        [FromForm] public List<IFormFile> NewImages { get; set; } = new();
    }
}
