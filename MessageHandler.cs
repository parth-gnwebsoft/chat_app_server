//using Fleck;
//using System.Text.Json;

//public class MessageHandler
//{
//    private readonly IConnectionManager _connManager;

//    // Get the ConnectionManager via constructor injection
//    public MessageHandler(IConnectionManager connManager)
//    {
//        _connManager = connManager;
//    }

//    // --- Logic for 'sendMessage' ---
//    public void HandleSendMessage(JsonElement payload, IWebSocketConnection socket)
//    {
//        var receiverId = payload.GetProperty("receiverId").GetString();
//        var receiverSocket = _connManager.GetSocket(receiverId);

//        if (receiverSocket != null)
//        {
//            SocketHelper.Send(receiverSocket, "getMessage", payload);
//        }
//        else
//        {
//            SocketHelper.Send(socket, "userOffline", new { receiverId });
//        }
//    }

//    // --- Logic for 'editMessage' ---
//    public void HandleEditMessage(JsonElement payload, IWebSocketConnection socket)
//    {
//        var isGroup = payload.GetProperty("isGroup").GetBoolean();
//        var chatId = payload.GetProperty("chatId").GetString();

//        if (isGroup)
//        {
//            var groupSockets = _connManager.GetSocketsInGroup(chatId);
//            foreach (var s in groupSockets)
//            {
//                SocketHelper.Send(s, "messageEdited", payload);
//            }
//        }
//        else
//        {
//            var receiverSocket = _connManager.GetSocket(chatId);
//            if (receiverSocket != null)
//            {
//                SocketHelper.Send(receiverSocket, "messageEdited", payload);
//            }
//            // Also send back to the sender for UI update
//            SocketHelper.Send(socket, "messageEdited", payload);
//        }
//    }

//    // ... Add HandleDeleteMessage, HandleSendReply, etc. ...
//}