namespace MinM_API.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<string?> UploadImageAsync(IFormFile file);
        string GetPublicIdFromUrl(string url);
        Task DeleteImageAsync(string publicId);
    }
}
