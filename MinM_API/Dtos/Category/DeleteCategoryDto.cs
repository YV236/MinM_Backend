namespace MinM_API.Dtos.Category
{
    public class DeleteCategoryDto
    {
        public string CategoryId { get; set; } = string.Empty;
        public DeleteOption Option { get; set; } = DeleteOption.ReassignToParent;
    }
}
