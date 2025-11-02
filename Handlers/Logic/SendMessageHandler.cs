// Handlers/Logic/SendMessageHandler.cs
using Fleck;
using System.Text.Json;
using Api;
using Models;
using System.Threading.Tasks;

namespace Handlers.Logic
{
    public class SendMessageHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository;

        public SendMessageHandler(
            IWebSocketStateService stateService,
            IMessageBroadcaster broadcaster,
            IMessageRepository messageRepository)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
            _messageRepository = messageRepository;
        }

        public Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            // ... (payload parsing and token checking) ...
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
                // 1. Call API
                ChatMessageResponse savedMessage = await _messageRepository.SaveMessageAsync(messageRequest, token);

                // --- 2. NEW LOGIC: Check ChannelType ---
                bool isGroup = savedMessage.ChannelType == "Group";

                if (isGroup)
                {
                    // --- Group Message Logic ---
                    if (savedMessage.ChannelID == null)
                    {
                        _broadcaster.Send(socket, "messageFailed", new { error = "Group message saved but missing ChannelID in response." });
                        return;
                    }

                    var groupId = savedMessage.ChannelID.ToString();
                    // Broadcast to everyone in the group (including sender)
                    if (groupId != null) { _broadcaster.BroadcastToGroup(groupId, "getMessage", savedMessage);}
                    
                }
                else
                {
                    // --- One-on-One Message Logic ---

                    // 1. Send back to SENDER
                    _broadcaster.Send(socket, "getMessage", savedMessage);

                    // 2. Send to RECEIVER
                    var receiverId = savedMessage.ReciverUserID?.ToString();
                    if (receiverId != null && _stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                    {
                        _broadcaster.Send(receiverSocket, "getMessage", savedMessage);
                    }
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