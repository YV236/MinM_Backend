using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Banner;
using MinM_API.Dtos.Cart;
using MinM_API.Dtos.Product;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Interfaces;
using System.Text.Json;

namespace MinM_API.Services.Implementations
{
    public class BannerService(DataContext context, ILogger<BannerService> logger, IPhotoService photoService) : IBannerService
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<ServiceResponse<List<GetBannerImagesDto>>> GetBannerImages()
        {
            try
            {
                var bannerImages = await context.BannerImages.OrderBy(bn => bn.SequenceNumber).ToListAsync();

                if (bannerImages.Count == 0)
                {
                    return ResponseFactory.Success(new List<GetBannerImagesDto>(), "No banners in database");
                }

                var getBannerImages = new List<GetBannerImagesDto>();

                foreach (var bannerImage in bannerImages)
                {
                    getBannerImages.Add(new GetBannerImagesDto
                    {
                        SequenceNumber = bannerImage.SequenceNumber,
                        ImageURL = bannerImage.ImageURL,
                        PageURL = bannerImage.PageURL,
                        ButtonText = bannerImage.ButtonText,
                        Text = bannerImage.Text,
                    });
                }

                return ResponseFactory.Success(getBannerImages, "Successful fetching of the banner photos");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fail: Error while fetching banner images");
                return ResponseFactory.Error(new List<GetBannerImagesDto>(), "Internal error");
            }
        }

        public async Task<ServiceResponse<int>> UpdateBanners(AddBannerImagesDto bannerImagesDto)
        {
            try
            {
                if (!bannerImagesDto.ExistingImages.IsNullOrEmpty())
                {
                    await ChangeImages(bannerImagesDto);
                }
                else
                {
                    var imagesToDelete = await context.BannerImages.ToListAsync();

                    foreach (var imageToRemove in imagesToDelete)
                    {
                        var publicId = photoService.GetPublicIdFromUrl(imageToRemove.ImageURL);
                        if (!string.IsNullOrEmpty(publicId))
                            await photoService.DeleteImageAsync(publicId);
                        context.BannerImages.Remove(imageToRemove);
                    }
                }

                var images = bannerImagesDto.NewImages;
                var sequenceNumbers = bannerImagesDto.ImageSequenceNumbers;
                var pageURLs = bannerImagesDto.PageURLs;
                var buttonTexts = bannerImagesDto.ButtonTexts;
                var texts = bannerImagesDto.Texts;

                for (int i = 0; i < images.Count; i++)
                {
                    var image = images[i];
                    var sequenceNumber = sequenceNumbers[i];
                    var PageURL = pageURLs[i];
                    var buttonText = buttonTexts[i];
                    var text = texts[i];
                    string? imageURL = null;

                    imageURL = await photoService.UploadImageAsync(image);

                    if (!string.IsNullOrEmpty(imageURL))
                    {
                        context.BannerImages.Add(new BannerImage
                        {
                            Id = Guid.NewGuid().ToString(),
                            SequenceNumber = sequenceNumber,
                            ImageURL = imageURL,
                            PageURL = PageURL,
                            ButtonText = buttonText,
                            Text = text,
                        });
                    }
                }

                var result = await context.SaveChangesAsync(); 
                return ResponseFactory.Success(result, "Banners updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating banner");
                return ResponseFactory.Error(0, "Internal error");
            }
        }

        private async Task ChangeImages(AddBannerImagesDto bannerImagesDto)
        {
            var existingImages = JsonSerializer.Deserialize<List<BannerPhoto>>(bannerImagesDto.ExistingImages, jsonOptions);

            var existingURLs = existingImages.Where(v => !string.IsNullOrEmpty(v.ImageURL)).Select(v => v.ImageURL).ToList();

            var imagesToRemove = await context.BannerImages.Where(bi => !existingURLs.Contains(bi.ImageURL)).ToListAsync();

            foreach (var imageToRemove in imagesToRemove)
            {
                var publicId = photoService.GetPublicIdFromUrl(imageToRemove.ImageURL);
                if (!string.IsNullOrEmpty(publicId))
                    await photoService.DeleteImageAsync(publicId);
                context.BannerImages.Remove(imageToRemove);
            }

            var all = context.BannerImages.ToList();
            context.BannerImages.RemoveRange(all);

            foreach (var image in existingImages)
            {
                context.BannerImages.Add(new BannerImage
                {
                    Id = Guid.NewGuid().ToString(),
                    SequenceNumber = image.SequenceNumber,
                    ImageURL = image.ImageURL,
                    PageURL = image.PageURL,
                    ButtonText = image.ButtonText,
                    Text = image.Text
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
