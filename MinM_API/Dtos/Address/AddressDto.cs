using System.Text.Json.Serialization;

namespace MinM_API.Dtos.Address
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(PostAddressDto), "post")]
    [JsonDerivedType(typeof(UserAddressDto), "user")]
    public abstract class AddressDto
    {
        protected AddressDto() { }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Region { get; set; }
    }
}
