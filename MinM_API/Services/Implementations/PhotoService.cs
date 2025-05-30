using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using MinM_API.Dtos;
using MinM_API.Extension;
using MinM_API.Models;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file.Length <= 0) return null;

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "MinM_app_folder",
                Transformation = new Transformation().Width(800).Height(800).Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return null;
            }

            return uploadResult.SecureUrl.ToString();
        }

        public string GetPublicIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var filename = Path.GetFileNameWithoutExtension(path);
            var folder = path.Split("/").Reverse().Skip(1).First();

            return $"{folder}/{filename}";
        }


        public async Task DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}
