using System.Text.Json.Serialization;

namespace MinM_API.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status : byte
    {
        Created = 0,
        Canceled = 1,
        Packaging = 2,
        Delivering = 3,
        Delivered = 4,
        Received = 5,
        Returned = 6,
    }
}
