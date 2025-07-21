using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Banner;
using MinM_API.Dtos.Cart;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class BannerService(DataContext context, ILogger<BannerService> logger, IPhotoService photoService) : IBannerService
    {
        public async Task<ServiceResponse<List<GetBannerImagesDto>>> GetBannerImages()
        {
            try
            {
                var bannerImages = await context.BannerImages.OrderBy(bn => bn.SequenceNumber).ToListAsync();

                var getBannerImages = new List<GetBannerImagesDto>();

                foreach (var bannerImage in bannerImages)
                {
                    getBannerImages.Add(new GetBannerImagesDto
                    {
                        URL = bannerImage.URL
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
                var images = bannerImagesDto.Images;
                var sequenceNumbers = bannerImagesDto.SequenceNumbers;

                // 1. Перевірка — списки мають бути однакової довжини
                if (images.Count != sequenceNumbers.Count)
                    return ResponseFactory.Error(0, "Images count і SequenceNumbers count must be equal!");

                // 2. Сформувати нову послідовність (SequenceNumber) — якісь фото з якими номерами потрібні
                var newSeqs = sequenceNumbers.ToHashSet();

                // 3. Забираємо старі банери
                var existingBanners = await context.BannerImages.ToListAsync();

                // 4. Видалити непотрібні банери
                var bannersToDelete = existingBanners.Where(b => !newSeqs.Contains(b.SequenceNumber)).ToList();
                foreach (var oldBanner in bannersToDelete)
                {
                    var publicId = photoService.GetPublicIdFromUrl(oldBanner.URL);
                    await photoService.DeleteImageAsync(publicId);

                    context.BannerImages.Remove(oldBanner);
                }

                // 5. Додаємо або оновлюємо банери — по індексу з масиву
                for (int i = 0; i < images.Count; i++)
                {
                    var sequenceNumber = sequenceNumbers[i];
                    var image = images[i];
                    var bannerInDb = existingBanners.FirstOrDefault(b => b.SequenceNumber == sequenceNumber);

                    if (bannerInDb != null)
                    {
                        var newUrl = await photoService.UploadImageAsync(image);

                        if (!string.IsNullOrEmpty(newUrl) && bannerInDb.URL != newUrl)
                        {
                            var publicId = photoService.GetPublicIdFromUrl(bannerInDb.URL);
                            await photoService.DeleteImageAsync(publicId);

                            bannerInDb.URL = newUrl;
                        }
                    }
                    else
                    {
                        var url = await photoService.UploadImageAsync(image);

                        if (!string.IsNullOrEmpty(url))
                        {
                            var newBanner = new BannerImage
                            {
                                Id = Guid.NewGuid().ToString(),
                                SequenceNumber = sequenceNumber,
                                URL = url
                            };
                            await context.BannerImages.AddAsync(newBanner);
                        }
                    }
                }

                // 6. Зберегти зміни
                await context.SaveChangesAsync();

                var count = await context.BannerImages.CountAsync();
                return ResponseFactory.Success(count, "Banners updated");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating banner");
                return ResponseFactory.Error(0, "Internal error");
            }
        }
    }
}
