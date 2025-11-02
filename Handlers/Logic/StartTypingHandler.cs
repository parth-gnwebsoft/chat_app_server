// Handlers/Logic/StartTypingHandler.cs
using Fleck;
using System.Text.Json;
using System.Threading.Tasks; // Keep this if IMessageHandler requires Task return

namespace Handlers.Logic
{
    public class StartTypingHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;

        public StartTypingHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
        }

        // ❌ Change 1: Remove 'async'
        public Task HandleAsync(IWebSocketConnection socket, JsonElement payload) // ⬅️ Changed
        {
            var isGroup = payload.GetProperty("isGroup").GetBoolean();
            var senderId = payload.GetProperty("senderId").GetString();
            var typingPayload = new { senderId, groupId = (string?)null };

            if (isGroup)
            {
                var groupId = payload.GetProperty("groupId").GetString();
                if(groupId != null)
                {
                    typingPayload = new { senderId, groupId };
                _broadcaster.BroadcastToGroup(groupId, "userTyping", typingPayload, excludeSocket: socket);
                }
                
            }
            else
            {
                var receiverId = payload.GetProperty("receiverId").GetString();
                if (_stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                {
                    _broadcaster.Send(receiverSocket, "userTyping", typingPayload);
                }
            }
            
            // ✅ Change 2: Return a completed Task synchronously
            return Task.CompletedTask; // ⬅️ Added
        }
    }
}