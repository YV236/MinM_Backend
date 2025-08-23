namespace MinM_API.Dtos.Review
{
    public class EditReviewDto
    {
        public required string Id { get; set; }

        public ushort Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
