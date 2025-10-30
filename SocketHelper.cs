//using Fleck;
//using System.Text.Json;

//public static class SocketHelper
//{
//    // Send a structured message to a single socket
//    public static void Send(IWebSocketConnection socket, string type, object payload)
//    {
//        var messageObject = new { type, payload };
//        var jsonMessage = JsonSerializer.Serialize(messageObject);
//        socket.Send(jsonMessage);
//    }
//}