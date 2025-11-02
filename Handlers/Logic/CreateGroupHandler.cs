// Handlers/Logic/CreateGroupHandler.cs
using Fleck;
using System.Text.Json;

namespace Handlers.Logic
{
    public class CreateGroupHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;

        public CreateGroupHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
        }

        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            var groupId = payload.GetProperty("groupId").GetString();
            if (groupId == null) return;

            var members = payload.GetProperty("members").EnumerateArray()
                .Select(e => e.GetString())
                .Where(id => id != null)
                .ToList();

            // The service handles all the complex logic of finding online users
            _stateService.CreateGroup(groupId, members!);

            // Notify all members of the new group
            _broadcaster.BroadcastToGroup(groupId, "newGroupCreated", payload);
        }
    }
}