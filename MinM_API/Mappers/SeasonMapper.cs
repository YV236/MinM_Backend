using MinM_API.Dtos.Season;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class SeasonMapper
    {
        public partial GetSeasonDto SeasonToGetSeasonDto(Season season);
    }
}
