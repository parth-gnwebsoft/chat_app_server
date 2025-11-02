// Handlers/WebSocketHandler.cs
using Fleck;
using System.Text.Json;
using Models; // We will create this namespace

namespace Handlers
{
    public class WebSocketHandler
    {
        private readonly IWebSocketStateService _stateService;
        private readonly IMessageBroadcaster _broadcaster;
        private readonly MessageHandlerFactory _handlerFactory;

        public WebSocketHandler(
            IWebSocketStateService stateService,
            IMessageBroadcaster broadcaster,
            MessageHandlerFactory handlerFactory)
        {
            _stateService = stateService;
            _broadcaster = broadcaster;
            _handlerFactory = handlerFactory;
        }

        // This is the method we pass to server.Start()
        public void OnNewConnection(IWebSocketConnection socket)
        {
            socket.OnOpen = () => OnOpen(socket);
            socket.OnClose = () => OnClose(socket);
            socket.OnMessage = message => OnMessage(socket, message);
        }

        private void OnOpen(IWebSocketConnection socket)
        {
            Console.WriteLine($"Someone connected with id = {socket.ConnectionInfo.Id}");
            _stateService.AddSocket(socket);
        }

        private void OnClose(IWebSocketConnection socket)
        {
            Console.WriteLine($"Someone disconnected with id = {socket.ConnectionInfo.Id}");

            // Check if the disconnected socket was a registered user
            var userId = _stateService.GetUserId(socket);

            _stateService.RemoveSocket(socket);

            // If they were, broadcast the new user list
            if (userId != null)
            {
                Console.WriteLine($"User {userId} removed.");
                var currentUsers = _stateService.GetOnlineUserIds();
                _broadcaster.Broadcast("getUsers", currentUsers);
            }
        }

        private void OnMessage(IWebSocketConnection socket, string message)
        {
            try
            {
                // Deserialize to a base model to find the type
                var baseMessage = JsonSerializer.Deserialize<BaseMessage>(message);
                if (baseMessage == null || string.IsNullOrEmpty(baseMessage.Type))
                {
                    Console.WriteLine("Invalid message format: 'type' is missing.");
                    return;
                }

                // Use the factory to get the correct handler
                var handler = _handlerFactory.GetHandler(baseMessage.Type);
                if (handler == null)
                {
                    Console.WriteLine($"No handler found for message type: {baseMessage.Type}");
                    return;
                }

                // Parse the full message for the payload
                var doc = JsonDocument.Parse(message);
                var payload = doc.RootElement.GetProperty("payload");

                // Execute the specific logic
                handler.HandleAsync(socket, payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                Console.WriteLine($"Raw message: {message}");
            }
        }
        // NEW: This is our async message handler
        private async Task HandleMessageAsync(IWebSocketConnection socket, string message)
        {
            try
            {
                var baseMessage = JsonSerializer.Deserialize<BaseMessage>(message);
                if (baseMessage == null || string.IsNullOrEmpty(baseMessage.Type))
                {
                    Console.WriteLine("Invalid message format: 'type' is missing.");
                    return;
                }

                var handler = _handlerFactory.GetHandler(baseMessage.Type);
                if (handler == null)
                {
                    Console.WriteLine($"No handler found for message type: {baseMessage.Type}");
                    return;
                }

                var doc = JsonDocument.Parse(message);
                var payload = doc.RootElement.GetProperty("payload");

                // Execute the specific logic asynchronously
                await handler.HandleAsync(socket, payload); // <-- Now awaited
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                Console.WriteLine($"Raw message: {message}");
            }
        }
    }
}
