// Handlers/Logic/StartTypingHandler.cs
using Fleck;
using System.Text.Json;

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

        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            var isGroup = payload.GetProperty("isGroup").GetBoolean();
            var senderId = payload.GetProperty("senderId").GetString();
            var typingPayload = new { senderId, groupId = (string?)null };

            if (isGroup)
            {
                var groupId = payload.GetProperty("groupId").GetString();
                typingPayload = new { senderId, groupId };
                _broadcaster.BroadcastToGroup(groupId, "userTyping", typingPayload, excludeSocket: socket);
            }
            else
            {
                var receiverId = payload.GetProperty("receiverId").GetString();
                if (_stateService.GetSocketByUserId(receiverId) is { } receiverSocket)
                {
                    _broadcaster.Send(receiverSocket, "userTyping", typingPayload);
                }
            }
        }
    }
}