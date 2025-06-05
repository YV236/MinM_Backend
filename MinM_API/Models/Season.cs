using Microsoft.EntityFrameworkCore;

namespace MinM_API.Models
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Season
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<Product> Products { get; set; } = [];
    }
}
