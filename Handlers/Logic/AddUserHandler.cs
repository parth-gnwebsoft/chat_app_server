// Handlers/Logic/AddUserHandler.cs
using Fleck;
using System.Text.Json;

namespace Handlers.Logic
{
    public class AddUserHandler : IMessageHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;

        public AddUserHandler(IWebSocketStateService stateService, IMessageBroadcaster broadcaster)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
        }

        public async Task HandleAsync(IWebSocketConnection socket, JsonElement payload)
        {
            var userId = payload.GetProperty("userId").GetString();
            if (userId != null)
            {
                _stateService.RegisterUser(userId, socket);
                Console.WriteLine($"User {userId} registered with socket {socket.ConnectionInfo.Id}");

                var currentUsers = _stateService.GetOnlineUserIds();
                // Broadcast to ALL sockets
                _broadcaster.Broadcast("getUsers", currentUsers);
                Console.WriteLine($"Current online users: {string.Join(", ", currentUsers)}");
            }
        }
    }
}