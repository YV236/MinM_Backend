namespace MinM_API.Dtos.Review
{
    public class AddReviewDto
    {
        public required string ProductId { get; set; }
        public ushort Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
