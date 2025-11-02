// Handlers/Logic/DeleteMessageHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks;
using Api;
using Models;

namespace Handlers.Logic
{
    public class DeleteMessageHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository; // <-- ADD

        // 1. Update constructor
        public DeleteMessageHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster, IMessageRepository messageRepository)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
            _messageRepository = messageRepository; // <-- ADD
        }

        // 2. Update HandleAsync
        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            string? token;
            ChatMessageRequest? messageRequest; // This will be the partial update

            // 3. Parse standard payload
            try
            {
                token = payload.GetProperty("authToken").GetString();
                messageRequest = payload.GetProperty("messageData").Deserialize<ChatMessageRequest>();
                if (string.IsNullOrEmpty(token) || messageRequest == null || messageRequest.MessageID == null)
                {
                    _broadcaster.Send(socket, "messageFailed", new { error = "Invalid payload, token, or missing messageID." });
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid payload format: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Invalid message format." });
                return;
            }

            // 4. Call API
            try
            {
                ChatMessageRequest updatedMessage = await _messageRepository.UpdateMessageAsync(messageRequest, token);

                // 5. Broadcast API response
                var isGroup = updatedMessage.ChannelType == "Group";

                if (isGroup)
                {
                    var groupId = updatedMessage.ChannelID.ToString();
                    _broadcaster.BroadcastToGroup(groupId, "messageDeleted", updatedMessage);
                }
                else
                {
                    // Send to the receiver
                    var receiverId = updatedMessage.ReciverUserID?.ToString();
                    if (receiverId != null && _stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                    {
                        _broadcaster.Send(receiverSocket, "messageDeleted", updatedMessage);
                    }
                    // Also send back to the sender
                    _broadcaster.Send(socket, "messageDeleted", updatedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save or send message: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Could not update message." });
            }
        }
    }
}