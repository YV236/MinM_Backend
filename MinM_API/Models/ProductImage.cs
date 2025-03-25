namespace MinM_API.Models
{
    public class ProductImage
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
    }
}
