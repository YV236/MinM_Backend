using Microsoft.AspNetCore.Mvc;
using MinM_API.Dtos.ProductVariant;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MinM_API.Dtos.Product
{
    public class AddProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [FromForm(Name = "ProductVariantsJson")]
        public string ProductVariantsJson { get; set; } = string.Empty;

        public string CategoryId { get; set; } = string.Empty;

        [FromForm(Name = "ProductColorsJson")]
        public string? ProductColorsJson { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;
        public List<IFormFile> Images { get; set; } = [];
        public List<int> ImageSequenceNumbers { get; set; } = [];
    }
}
