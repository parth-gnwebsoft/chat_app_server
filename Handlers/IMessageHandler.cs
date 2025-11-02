// Handlers/IMessageHandler.cs
using Fleck;
using System.Text.Json;

public interface IMessageHandler
{
    Task HandleAsync(IWebSocketConnection socket, JsonElement payload);
}