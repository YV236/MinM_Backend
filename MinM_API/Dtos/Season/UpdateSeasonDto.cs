namespace MinM_API.Dtos.Season
{
    public class UpdateSeasonDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<string> ProductIds { get; set; } = [];
    }
}
