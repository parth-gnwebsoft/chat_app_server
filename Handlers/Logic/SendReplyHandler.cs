// Handlers/Logic/SendReplyHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks;
using Api;
using Models;

namespace Handlers.Logic
{
    public class SendReplyHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository;

        // 1. Update constructor
        public SendReplyHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster, IMessageRepository messageRepository)
        {
            _stateService = stateService;
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

            // 4. Save to API
            try
            {
                ChatMessageResponse savedMessage = await _messageRepository.SaveMessageAsync(messageRequest, token);

                // 5. Broadcast the API's response
                // We infer 'isGroup' from the saved message
                var isGroup = savedMessage.ChannelType == "Group";

                if (isGroup)
                {
                    var groupId = savedMessage.ChannelID.ToString();
                    if (groupId != null)
                    {
                        _broadcaster.BroadcastToGroup(groupId, "getReply", savedMessage);
                    }
                }
                else
                {
                    _broadcaster.Send(socket, "getReply", savedMessage);
                    var receiverId = savedMessage.ReciverUserID?.ToString(); 

                    if (receiverId != null && _stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                    {
                        _broadcaster.Send(receiverSocket, "getReply", savedMessage);
                    }
                    else if (receiverId != null)
                    {
                        _broadcaster.Send(socket, "userOffline", new { receiverId });
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