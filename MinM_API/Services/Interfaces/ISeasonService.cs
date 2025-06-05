using MinM_API.Dtos.Season;
using MinM_API.Dtos;

namespace MinM_API.Services.Interfaces
{
    public interface ISeasonService
    {
        Task<ServiceResponse<int>> AddSeason(AddSeasonDto addSeasonDto);
        Task<ServiceResponse<List<GetSeasonDto>>> GetAllSeasons();
        Task<ServiceResponse<GetSeasonDto>> GetSeasonById(string id);
        Task<ServiceResponse<GetSeasonDto>> GetSeasonBySlug(string slug);
        Task<ServiceResponse<int>> UpdateSeason(UpdateSeasonDto updateSeasonDto);
    }
}
