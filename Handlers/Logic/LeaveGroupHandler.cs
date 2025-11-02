// Handlers/Logic/LeaveGroupHandler.cs
using Fleck;
using System.Text.Json;

namespace Handlers.Logic
{
    public class LeaveGroupHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;

        public LeaveGroupHandler(IWebSocketStateService stateService)
        {
            _stateService = stateService;
        }

        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            var groupId = payload.GetProperty("groupId").GetString();
            if (groupId == null) return;

            _stateService.LeaveGroup(groupId, socket);
            Console.WriteLine($"Socket {socket.ConnectionInfo.Id} left group {groupId}");
        }
    }
}