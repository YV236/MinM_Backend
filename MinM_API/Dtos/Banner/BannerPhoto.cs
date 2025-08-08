namespace MinM_API.Dtos.Banner
{
    public class BannerPhoto
    {
        public int SequenceNumber { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public string PageURL { get; set; } = string.Empty;

        public string? ButtonText { get; set; } = string.Empty;
        public string? Text { get; set; } = string.Empty;
    }
}
