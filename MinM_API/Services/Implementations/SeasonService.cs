using MinM_API.Dtos;
using MinM_API.Dtos.Season;
using MinM_API.Services.Interfaces;

namespace MinM_API.Services.Implementations
{
    public class SeasonService : ISeasonService
    {
        public Task<ServiceResponse<int>> AddSeason(AddSeasonDto addSeasonDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<List<GetSeasonDto>>> GetAllSeasons()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetSeasonDto>> GetSeasonById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<GetSeasonDto>> GetSeasonBySlug(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<int>> UpdateSeason(UpdateSeasonDto updateSeasonDto)
        {
            throw new NotImplementedException();
        }
    }
}
