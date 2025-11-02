// Handlers/Logic/AddReactionHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks;
using Api;
using Models;

namespace Handlers.Logic
{
    public class AddReactionHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly IMessageRepository _messageRepository;

        // 1. Update constructor
        public AddReactionHandler(
            IWebSocketStateService stateService,
            IMessageBroadcaster broadcaster,
            IMessageRepository messageRepository)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
            _messageRepository = messageRepository;
        }

        // 2. Update HandleAsync
        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            string? token;
            ReactionRequest? reactionRequest;
            JsonElement routingInfo;

            // 3. Parse the new composite payload
            try
            {
                token = payload.GetProperty("authToken").GetString();
                reactionRequest = payload.GetProperty("reactionData").Deserialize<ReactionRequest>();
                routingInfo = payload.GetProperty("routingInfo"); // <-- Context for broadcasting

                if (string.IsNullOrEmpty(token) || reactionRequest == null)
                {
                    _broadcaster.Send(socket, "messageFailed", new { error = "Invalid payload, token, or reactionData." });
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
                ReactionResponse savedReaction = await _messageRepository.AddReactionAsync(reactionRequest, token);

                // 5. Broadcast API response using 'routingInfo' 
                var isGroup = routingInfo.GetProperty("isGroup").GetBoolean();

                if (isGroup)  
                {
                    // 'chatId' is the ChannelID (as a string)
                    var groupId = routingInfo.GetProperty("chatId").GetString();
                    if (groupId != null)
                    {
                        // Broadcast to everyone in the group
                        _broadcaster.BroadcastToGroup(groupId, "newReaction", savedReaction);
                    }
                }
                else
                {
                    // 'receiverId' is the UserID of the other person (as a string)
 
                    var receiverId = routingInfo.GetProperty("receiverId").GetString();
                    

                    if (receiverId != null && _stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                    {
                        // Send to the specific receiver
                        _broadcaster.Send(receiverSocket, "newReaction", savedReaction);
                    }
                    // Also send back to the sender so their UI updates
                    _broadcaster.Send(socket, "newReaction", savedReaction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add reaction: {ex.Message}");
                _broadcaster.Send(socket, "messageFailed", new { error = "Could not add reaction." });
            }
        }
    }
}