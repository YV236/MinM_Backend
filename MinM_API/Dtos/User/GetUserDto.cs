namespace MinM_API.Dtos.User
{
    public record GetUserDto
    {
        public string? UserName { get; set; }
        public string? Slug { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? Email { get; set; }
        public Address? Address { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
