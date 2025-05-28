namespace MinM_API.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<string?> UploadImageAsync(IFormFile file);
    }
}
