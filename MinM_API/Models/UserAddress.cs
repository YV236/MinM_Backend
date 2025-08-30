namespace MinM_API.Models
{
    public class UserAddress : Address
    {
        public string UserId { get; set; } = "guest";
        public virtual User? User { get; set; }
        public string Street { get; set; } = string.Empty;
        public string HomeNumber { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
    }
}
