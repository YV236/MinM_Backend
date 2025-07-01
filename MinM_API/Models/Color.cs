using Microsoft.EntityFrameworkCore;
using MinM_API.Migrations;
using System.ComponentModel.DataAnnotations;

namespace MinM_API.Models
{
    [Index(nameof(ColorHex), IsUnique = true)]
    public class Color
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
        public string ColorHex { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
