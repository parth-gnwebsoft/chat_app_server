// Handlers/Logic/SendGroupMessageHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks;
using Api;
using Models;

namespace Handlers.Logic
{
    public class SendGroupMessageHandler : IMessageHandler
    {
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository;

        // 1. Update constructor
        public SendGroupMessageHandler(IMessageBroadcaster broadcaster, IMessageRepository messageRepository)
        {
            _broadcaster = broadcaster;
            _messageRepository = messageRepository;
        }

        // 2. Implement async method
        public Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            string? token;
            JsonElement messageDataPayload;
            ChatMessageRequest? messageRequest;

            // 3. Parse the standard payload
            try
            {
                token = payload.GetProperty("authToken").GetString();
                messageDataPayload = payload.GetProperty("messageData");
                messageRequest = messageDataPayload.Deserialize<ChatMessageRequest>();

                if (string.IsNullOrEmpty(token) || messageRequest == null || messageRequest.ChannelID == null)
                {
                    _broadcaster.Send(socket, "messageFailed", new { error = "Invalid payload, token, or missing channelID." });
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid payload format: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Invalid message format." });
                return;
            }

            // 4. Save to API
            try
            {
                ChatMessageResponse savedMessage = await _messageRepository.SaveMessageAsync(messageRequest, token);

                // 5. Broadcast the API's response
                var groupId = savedMessage.ChannelID.ToString();
                if (groupId != null)
                {
                    _broadcaster.BroadcastToGroup(groupId, "getGroupMessage", savedMessage);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save or send message: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Could not save message to server." });
            }
        }
    }
}