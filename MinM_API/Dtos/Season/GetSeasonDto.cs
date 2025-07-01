using MinM_API.Dtos.Product;

namespace MinM_API.Dtos.Season
{
    public class GetSeasonDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<GetProductDto> Products { get; set; } = [];
    }
}
