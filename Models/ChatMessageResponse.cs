// Models/ChatMessageRequest.cs
using System.Text.Json.Serialization;

namespace Models
{
    // This class models the JSON data you want to send to your API
    public class ChatMessageResponse
    {
        [JsonPropertyName("MessageID")]
        public int? MessageID { get; set; }

        [JsonPropertyName("channelType")]
        public string? ChannelType { get; set; }
        [JsonPropertyName("ChannelID")]
        public int? ChannelID { get; set; }

        [JsonPropertyName("SenderUserID")]
        public int? SenderUserID { get; set; }

        // Note: Your API model has a typo ("reciver"). We match it exactly.
        [JsonPropertyName("ReciverUserID")]
        public int? ReciverUserID { get; set; }

        [JsonPropertyName("Content")]
        public string? Content { get; set; }

        [JsonPropertyName("MessageType")]
        public string? MessageType { get; set; }

        [JsonPropertyName("ReplyToMessageID")]
        public int? ReplyToMessageID { get; set; }

        [JsonPropertyName("ForwardedFromMessageID")]
        public int? ForwardedFromMessageID { get; set; }

        [JsonPropertyName("SentDateTime")]
        public string? SentDateTime { get; set; } // Using string to match your model

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("DeletedDateTime")]
        public string? DeletedDateTime { get; set; }

        [JsonPropertyName("DeletedByUserID")]
        public int? DeletedByUserID { get; set; }

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