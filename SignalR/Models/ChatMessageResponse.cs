// Models/ChatMessageRequest.cs
using System.Text.Json.Serialization;

namespace Models
{
    // This class models the JSON data you want to send to your API
    public class ChatMessageResponse
    {
        [JsonPropertyName("messageID")]
        public int? MessageID { get; set; }

        [JsonPropertyName("channelType")]
        public string? ChannelType { get; set; }
        [JsonPropertyName("channelID")]
        public int? ChannelID { get; set; }

        [JsonPropertyName("senderUserID")]
        public int? SenderUserID { get; set; }

        [JsonPropertyName("messageUrls")]
        public String? messageUrls { get; set; }

        // Note: Your API model has a typo ("reciver"). We match it exactly.
        [JsonPropertyName("reciverUserID")]
        public int? ReciverUserID { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }


         [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonPropertyName("messageType")]
        public string? MessageType { get; set; }

        [JsonPropertyName("replyToMessageID")]
        public int? ReplyToMessageID { get; set; }

        [JsonPropertyName("forwardedFromMessageID")]
        public int? ForwardedFromMessageID { get; set; }

        [JsonPropertyName("sentDateTime")]
        public string? SentDateTime { get; set; } // Using string to match your model

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("deletedDateTime")]
        public string? DeletedDateTime { get; set; }

        [JsonPropertyName("deletedByUserID")]
        public int? DeletedByUserID { get; set; }

        [JsonPropertyName("created")]
        public string? Created { get; set; }

        [JsonPropertyName("modified")]
        public string? Modified { get; set; }

        [JsonPropertyName("createdByUserID")]
        public int? CreatedByUserID { get; set; }

        [JsonPropertyName("modifiedByUserID")]
        public int? ModifiedByUserID { get; set; }


        [JsonPropertyName("mobileMessageID")]
        public string? mobileMessageID { get; set; }
    }
}