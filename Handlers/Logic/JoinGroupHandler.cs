// Handlers/Logic/JoinGroupHandler.cs
using Fleck;
using System.Text.Json;

namespace Handlers.Logic
{
    public class JoinGroupHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;

        public JoinGroupHandler(IWebSocketStateService stateService)
        {
            _stateService = stateService;
        }

        public Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            var groupId = payload.GetProperty("groupId").GetString();
            if (groupId == null) return;

            _stateService.JoinGroup(groupId, socket);
            Console.WriteLine($"Socket {socket.ConnectionInfo.Id} joined group {groupId}");
        }
    }
}