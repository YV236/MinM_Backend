using System.Text.Json.Serialization;

namespace MinM_API.Dtos.Category
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeleteOption
    {
        CascadeDelete,
        ReassignToParent,
        Orphan
    }
}
