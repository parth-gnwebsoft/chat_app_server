// Models/ReactionResponse.cs
using System.Text.Json.Serialization;

namespace Models
{
    public class ReactionResponse
    {
        [JsonPropertyName("reactionID")]
        public int? ReactionID { get; set; }

        [JsonPropertyName("messageID")]
        public int? MessageID { get; set; }

        [JsonPropertyName("reactionType")]
        public string? ReactionType { get; set; }

        [JsonPropertyName("reaction")]
        public string? Reaction { get; set; }

        [JsonPropertyName("userID")]
        public int? UserID { get; set; }

        [JsonPropertyName("userDisplayName")]
        public string? UserDisplayName { get; set; } // Included

        [JsonPropertyName("userprofileImage")]
        public string? UserprofileImage { get; set; } // Included

        [JsonPropertyName("reactionDateTime")]
        public string? ReactionDateTime { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("modified")]
        public string? Modified { get; set; }

        [JsonPropertyName("createdByUserID")]
        public int? CreatedByUserID { get; set; }

        [JsonPropertyName("modifiedByUserID")]
        public int? ModifiedByUserID { get; set; }
    }
}