// Models/ReactionRequest.cs
using System.Text.Json.Serialization;

namespace Models
{
    public class ReactionRequest
    {
        [JsonPropertyName("reactionType")]
        public string? ReactionType { get; set; }

        [JsonPropertyName("reaction")]
        public string? Reaction { get; set; }

        [JsonPropertyName("userID")]
        public int? UserID { get; set; }

        [JsonPropertyName("reactionDateTime")]
        public string? ReactionDateTime { get; set; }

        [JsonPropertyName("channelID")]
        public int? ChannelID { get; set; } // Included as per your list

        [JsonPropertyName("messageID")]
        public int? MessageID { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("modified")]
        public string? Modified { get; set; }

        [JsonPropertyName("createdByUserID")]
        public int? CreatedByUserID { get; set; }

        [JsonPropertyName("modifiedByUserID")]
        public int? ModifiedByUserID { get; set; }
        //[JsonPropertyName("channelType")]
        //public int? channelType { get; set; }
    }
}