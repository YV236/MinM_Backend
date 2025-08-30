namespace MinM_API.Dtos.Address
{
    public class PostAddressDto : AddressDto
    {
        public PostAddressDto() { }
        public string PostDepartment { get; set; } = string.Empty;
    }
}
