using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MinM_API.Models
{
    public class User : IdentityUser
    {
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? AddressId { get; set; }
        public virtual Address? Address { get; set; }
        public virtual List<WishlistItem>? WishList { get; set; }
        public DateTime DateOfCreation { get; set; }
        public virtual List<Product>? Cart { get; set; }
        public virtual List<Order>? History { get; set; }
    }
}
