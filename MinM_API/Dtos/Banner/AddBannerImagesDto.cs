using Microsoft.AspNetCore.Mvc;

namespace MinM_API.Dtos.Banner
{
    public record AddBannerImagesDto
    {
        [FromForm(Name = "ExistingImages")] public string? ExistingImages { get; set; } = string.Empty;

        [FromForm] public List<IFormFile> NewImages { get; set; } = [];
        [FromForm] public List<int> ImageSequenceNumbers { get; set; } = [];
        [FromForm] public List<string> PageURLs { get; set; } = [];

        [FromForm] public List<string>? ButtonTexts { get; set; } = [];
        [FromForm] public List<string>? Texts { get; set; } = [string.Empty];
    }
}
