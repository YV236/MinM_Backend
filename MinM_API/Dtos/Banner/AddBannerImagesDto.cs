using Microsoft.AspNetCore.Mvc;

namespace MinM_API.Dtos.Banner
{
    public record AddBannerImagesDto([property: FromForm] List<IFormFile> Images, [property: FromForm] List<int> SequenceNumbers);
}
