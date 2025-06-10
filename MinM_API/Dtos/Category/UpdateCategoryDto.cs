namespace MinM_API.Dtos.Category
{
    public class UpdateCategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? ParentCategoryId { get; set; }

        public string? ExistingImageURL { get; set; } = string.Empty;

        public IFormFile? NewImage { get; set; }
    }
}
