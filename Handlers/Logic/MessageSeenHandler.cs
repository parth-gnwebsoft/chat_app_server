// Handlers/Logic/MessageSeenHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks;
using Api;
using Models;

namespace Handlers.Logic
{
    public class MessageSeenHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository;

        public MessageSeenHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster, IMessageRepository messageRepository)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
            _messageRepository = messageRepository;
        }

        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            // ... (Payload parsing and token checking is the same) ...
            string? token;
            ChatMessageRequest? messageRequest;
            try
            {
                token = payload.GetProperty("authToken").GetString();
                messageRequest = payload.GetProperty("messageData").Deserialize<ChatMessageRequest>();
                if (string.IsNullOrEmpty(token) || messageRequest == null)
                {
                    _broadcaster.Send(socket, "messageFailed", new { error = "Invalid payload or token." });
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid payload format: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Invalid message format." });
                return;
            }

            try
            {
                ChatMessageRequest updatedMessage = await _messageRepository.UpdateMessageAsync(messageRequest, token);

                var isGroup = updatedMessage.ChannelType == "Group";

                if (isGroup)
                {
                    var groupId = updatedMessage.ChannelID.ToString();

                    // --- FIX #1 ---
                    // OLD: _broadcaster.BroadcastToGroup(groupId, "updateMessageSeen", updatedMessage, excludeSocket: socket);
                    // NEW: Broadcast to everyone, including the person who sent the "seen" event.
                    _broadcaster.BroadcastToGroup(groupId, "updateMessageSeen", updatedMessage);
                }
                else
                {
                    // --- FIX #2 ---
                    // 1. Send the update back to the SENDER (the person who marked it as "seen")
                    _broadcaster.Send(socket, "updateMessageSeen", updatedMessage);

                    // 2. Notify the ORIGINAL message author
                    var senderId = updatedMessage.SenderUserID?.ToString();
                    if (senderId != null && _stateService.GetSocketByUserId(senderId) is { } senderSocket)
                    {
                        _broadcaster.Send(senderSocket, "updateMessageSeen", updatedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update message: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Could not update message." });
            }
        }
    }
}