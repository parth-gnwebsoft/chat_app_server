using System.Text.Json.Serialization;

namespace Models
{
    public class UpdateOnlineStatusRequest
    {
        [JsonPropertyName("userID")]
        public int? UserID { get; set; }

        [JsonPropertyName("isOnlineInChat")]
        public bool IsOnlineInChat { get; set; }
    }
}