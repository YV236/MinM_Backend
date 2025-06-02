using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos.ProductVariant;

namespace MinM_API.Dtos.Products
{
    public class UpdateProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [FromForm(Name = "ProductVariantsJson")]
        public string ProductVariantsJson { get; set; } = string.Empty;

        public string CategoryId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public List<string> ExistingImageUrls { get; set; } = [];

        public List<IFormFile> NewImages { get; set; } = [];
    }
}
