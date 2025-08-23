using MinM_API.Dtos.Product;

namespace MinM_API.Dtos.Review
{
    public class GetReviewDto
    {
        public required string Id { get; set; }

        public ushort Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
