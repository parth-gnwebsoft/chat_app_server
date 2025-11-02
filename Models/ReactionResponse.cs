// Models/ReactionResponse.cs
using System.Text.Json.Serialization;

namespace Models
{
    public class ReactionResponse
    {
        [JsonPropertyName("ReactionID")]
        public int? ReactionID { get; set; }

        [JsonPropertyName("MessageID")]
        public int? MessageID { get; set; }

        [JsonPropertyName("ReactionType")]
        public string? ReactionType { get; set; }

        [JsonPropertyName("Reaction")]
        public string? Reaction { get; set; }

        [JsonPropertyName("UserID")]
        public int? UserID { get; set; }

        [JsonPropertyName("UserDisplayName")]
        public string? UserDisplayName { get; set; } // Included

        [JsonPropertyName("UserprofileImage")]
        public string? UserprofileImage { get; set; } // Included

        [JsonPropertyName("ReactionDateTime")]
        public string? ReactionDateTime { get; set; }

        [JsonPropertyName("Created")]
        public string? Created { get; set; }

        [JsonPropertyName("Modified")]
        public string? Modified { get; set; }

        [JsonPropertyName("CreatedByUserID")]
        public int? CreatedByUserID { get; set; }

        [JsonPropertyName("ModifiedByUserID")]
        public int? ModifiedByUserID { get; set; }
    }
}