namespace MinM_API.Models
{
    public class BannerImage
    {
        public string Id { get; set; }

        public int SequenceNumber { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public string PageURL { get; set; } = string.Empty;

        public string? ButtonText { get; set; } = string.Empty;
        public string? Text { get; set; } = string.Empty;
    }
}
