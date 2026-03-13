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
                        PhoneImageURL = bannerImage.PhoneImageURL,
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
                var phoneImages = bannerImagesDto.PhoneImages;
                var sequenceNumbers = bannerImagesDto.ImageSequenceNumbers;
                var pageURLs = bannerImagesDto.PageURLs;
                var buttonTexts = bannerImagesDto.ButtonTexts;
                var texts = bannerImagesDto.Texts;

                for (int i = 0; i < images.Count; i++)
                {
                    var image = images[i];
                    var phoneImage = phoneImages[i];
                    var sequenceNumber = sequenceNumbers[i];
                    var PageURL = pageURLs[i];
                    var buttonText = buttonTexts[i];
                    var text = texts[i];
                    string? imageURL = null;

                    imageURL = await photoService.UploadImageAsync(image);
                    string? phoneImageURL = await photoService.UploadImageAsync(phoneImage);

                    if (!string.IsNullOrEmpty(imageURL))
                    {
                        context.BannerImages.Add(new BannerImage
                        {
                            Id = Guid.NewGuid().ToString(),
                            SequenceNumber = sequenceNumber,
                            ImageURL = imageURL,
                            PhoneImageURL = phoneImageURL,
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
            var existingImages = JsonSerializer.Deserialize<List<BannerPhoto>>(
                bannerImagesDto.ExistingImages,
                jsonOptions
            );

            if (existingImages == null || existingImages.Count == 0)
                return;

            var bannersInDb = await context.BannerImages.ToListAsync();

            foreach (var image in existingImages)
            {
                var banner = bannersInDb
                    .FirstOrDefault(b => b.SequenceNumber == image.SequenceNumber);

                if (banner == null)
                {
                    context.BannerImages.Add(new BannerImage
                    {
                        Id = Guid.NewGuid().ToString(),
                        SequenceNumber = image.SequenceNumber,
                        ImageURL = image.ImageURL,
                        PhoneImageURL = image.PhoneImageURL,
                        PageURL = image.PageURL,
                        ButtonText = image.ButtonText,
                        Text = image.Text
                    });

                    continue;
                }

                // PC IMAGE UPDATE
                if (banner.ImageURL != image.ImageURL)
                {
                    var publicId = photoService.GetPublicIdFromUrl(banner.ImageURL);

                    if (!string.IsNullOrEmpty(publicId))
                        await photoService.DeleteImageAsync(publicId);

                    banner.ImageURL = image.ImageURL;
                }

                // PHONE IMAGE UPDATE
                if (banner.PhoneImageURL != image.PhoneImageURL)
                {
                    var publicId = photoService.GetPublicIdFromUrl(banner.PhoneImageURL);

                    if (!string.IsNullOrEmpty(publicId))
                        await photoService.DeleteImageAsync(publicId);

                    banner.PhoneImageURL = image.PhoneImageURL;
                }

                // OTHER FIELDS
                banner.PageURL = image.PageURL;
                banner.ButtonText = image.ButtonText;
                banner.Text = image.Text;
            }

            // DELETE REMOVED BANNERS
            var sequenceNumbers = existingImages.Select(e => e.SequenceNumber).ToList();

            var bannersToDelete = bannersInDb
                .Where(b => !sequenceNumbers.Contains(b.SequenceNumber))
                .ToList();

            foreach (var banner in bannersToDelete)
            {
                var pcPublicId = photoService.GetPublicIdFromUrl(banner.ImageURL);
                if (!string.IsNullOrEmpty(pcPublicId))
                    await photoService.DeleteImageAsync(pcPublicId);

                var phonePublicId = photoService.GetPublicIdFromUrl(banner.PhoneImageURL);
                if (!string.IsNullOrEmpty(phonePublicId))
                    await photoService.DeleteImageAsync(phonePublicId);

                context.BannerImages.Remove(banner);
            }

            await context.SaveChangesAsync();
        }
    }
}
