using MinM_API.Dtos;
using MinM_API.Dtos.Banner;

namespace MinM_API.Services.Interfaces
{
    public interface IBannerService
    {
        Task<ServiceResponse<int>> UpdateBanners(AddBannerImagesDto bannerImagesDtos);
        Task<ServiceResponse<List<GetBannerImagesDto>>> GetBannerImages();
    }
}
