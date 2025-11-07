// Models/BaseMessage.cs
using System.Text.Json.Serialization;

namespace Models
{
    // Used to read the "type" property from any message
    public class BaseMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}