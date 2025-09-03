using System.Text.Json.Serialization;

namespace MinM_API.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status : byte
    {
        Created = 0,
        Canceled = 1,
        Paid = 2,
        Pending = 3,
        Delivering = 4,
        Received = 5,
        Returned = 6,
        Failed = 7
    }
}
