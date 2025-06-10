namespace MinM_API.Dtos.Category
{
    public record AddCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? ParentCategoryId { get; set; }

        public IFormFile Image { get; set; }
    }
}
