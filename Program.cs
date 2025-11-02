//using Fleck;
//using System.Collections.Concurrent;
//using System.Text.Json;

//class Program
//{
//    // --- 1. Define our State Management (must be thread-safe) ---

//    // This holds all connected clients
//    // We use a List<> locked by an object, as it's simpler for broadcast
//    private static List<IWebSocketConnection> allSockets = new();

//    // Replaces your 'onlineUsers' object: Maps <UserId, SocketConnection>
//    private static ConcurrentDictionary<string, IWebSocketConnection> onlineUsers = new();

//    // We must manually create "Groups": Maps <GroupId, List_of_Sockets>
//    private static ConcurrentDictionary<string, List<IWebSocketConnection>> groups = new();
//    private static readonly object socketLock = new();
//    private static readonly object groupLock = new();

//    static void Main()
//    {
//        // --- 2. Start the Server --- 
//        Console.WriteLine("Starting Fleck server...");

//        // Listen on all network interfaces
//        var port = Environment.GetEnvironmentVariable("PORT") ?? "2000";
//        var server = new WebSocketServer($"ws://0.0.0.0:{port}");

//        server.Start(socket =>
//        {
//            // === 3. OnOpen (Client Connected) ===
//            socket.OnOpen = () =>
//            {
//                Console.WriteLine($"Someone connected with id = {socket.ConnectionInfo.Id}");
//                lock (socketLock)
//                {
//                    allSockets.Add(socket);
//                }
//            };

//            // === 4. OnClose (Client Disconnected) ===
//            socket.OnClose = () =>
//            {
//                Console.WriteLine($"Someone disconnected with id = {socket.ConnectionInfo.Id}");

//                // Remove from main list
//                lock (socketLock)
//                {
//                    allSockets.Remove(socket);
//                }

//                // Find and remove from onlineUsers
//                var userIdToRemove = onlineUsers.FirstOrDefault(x => x.Value == socket).Key;
//                if (userIdToRemove != null)
//                {
//                    onlineUsers.TryRemove(userIdToRemove, out _);
//                    Console.WriteLine($"User {userIdToRemove} removed.");

//                    // Broadcast the new user list
//                    var currentUsers = onlineUsers.Keys.ToList();
//                    Broadcast(new { type = "getUsers", payload = currentUsers });
//                }

//                // Remove from all groups (this is complex but necessary)
//                lock (groupLock)
//                {
//                    foreach (var group in groups)
//                    {
//                        if (group.Value.Contains(socket))
//                        {
//                            group.Value.Remove(socket);
//                        }
//                    }
//                }
//            };

//            // === 5. OnMessage (Data Received) ===
//            socket.OnMessage = message =>
//            {
//                try
//                {
//                    // We use System.Text.Json.JsonElement to read the "type"
//                    var doc = JsonDocument.Parse(message);
//                    var root = doc.RootElement;
//                    Console.WriteLine("DATA ");
//                    // Our custom "event name"
//                    var messageType = root.GetProperty("type").GetString();
//                    // The rest of the data
//                    var payload = root.GetProperty("payload");
//                    //{
//                        //type: addUser
//                        //payload:{
//                        //           
//                        //}
//                    //}
//                    // This switch statement is our "Event Router"
//                    switch (messageType)
//                    {
//                        // Replaces socket.on('addUser', ...)
//                        case "addUser":
//                            {
//                                var userId = payload.GetProperty("userId").GetString();
//                                if (userId != null)
//                                {
//                                    onlineUsers[userId] = socket; // Add or update
//                                    Console.WriteLine($"User {userId} registered with socket {socket.ConnectionInfo.Id}");

//                                    var currentUsers = onlineUsers.Keys.ToList();
//                                    // io.emit('getUsers', ...) -> Broadcast to ALL sockets
//                                    Broadcast(new { type = "getUsers", payload = currentUsers });
//                                    Console.WriteLine($"Current online users: {string.Join(", ", currentUsers)}");
//                                }
//                                break;
//                            }

//                        // Replaces socket.on('sendMessage', ...)
//                        case "sendMessage":
//                            {
//                                Console.WriteLine("DATA ");
//                                var senderId = payload.GetProperty("senderId").GetString();
//                                var receiverId = payload.GetProperty("receiverId").GetString();

//                                if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                {
//                                    // io.to(receiverSocketId).emit("getMessage", ...)
//                                    Send(receiverSocket, "getMessage", payload);
//                                }
//                                else
//                                {
//                                    // socket.emit("userOffline", ...)
//                                    Send(socket, "userOffline", new { receiverId });
//                                }
//                                break;
//                            }

//                        // Replaces socket.on('sendReply', ...)
//                        case "sendReply":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();

//                                if (isGroup)
//                                {
//                                    var groupId = payload.GetProperty("groupId").GetString();
//                                    // socket.to(groupId).emit("getReply", ...)
//                                    BroadcastToGroup(groupId, "getReply", payload, excludeSocket: socket);
//                                }
//                                else
//                                {
//                                    var receiverId = payload.GetProperty("receiverId").GetString();
//                                    if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                    {
//                                        // io.to(receiverSocketId).emit("getReply", ...)
//                                        Send(receiverSocket, "getReply", payload);
//                                    }
//                                    else
//                                    {
//                                        Send(socket, "userOffline", new { receiverId });
//                                    }
//                                }
//                                break;
//                            }

//                        // Replaces socket.on('joinGroup', ...)
//                        case "joinGroup":
//                            {
//                                var groupId = payload.GetProperty("groupId").GetString();
//                                if (groupId == null) break;

//                                lock (groupLock)
//                                {
//                                    // Get or create the group list
//                                    if (!groups.ContainsKey(groupId))
//                                    {
//                                        groups[groupId] = new List<IWebSocketConnection>();
//                                    }
//                                    // Add socket to group if not already in it
//                                    if (!groups[groupId].Contains(socket))
//                                    {
//                                        groups[groupId].Add(socket);
//                                    }
//                                }
//                                Console.WriteLine($"Socket {socket.ConnectionInfo.Id} joined group {groupId}");
//                                break;
//                            }

//                        // Replaces socket.on('sendGroupMessage', ...)
//                        case "sendGroupMessage":
//                            {
//                                var groupId = payload.GetProperty("groupId").GetString();
//                                // socket.to(groupId).emit("getGroupMessage", ...)
//                                BroadcastToGroup(groupId, "getGroupMessage", payload, excludeSocket: socket);
//                                break;
//                            }

//                        // Replaces socket.on('leaveGroup', ...)
//                        case "leaveGroup":
//                            {
//                                var groupId = payload.GetProperty("groupId").GetString();
//                                if (groupId == null) break;

//                                lock (groupLock)
//                                {
//                                    if (groups.ContainsKey(groupId))
//                                    {
//                                        groups[groupId].Remove(socket);
//                                    }
//                                }
//                                Console.WriteLine($"Socket {socket.ConnectionInfo.Id} left group {groupId}");
//                                break;
//                            }

//                        // Replaces socket.on('createGroup', ...)
//                        case "createGroup":
//                            {
//                                var groupId = payload.GetProperty("groupId").GetString();
//                                var members = payload.GetProperty("members").EnumerateArray()
//                                                    .Select(e => e.GetString()).ToList();

//                                if (groupId == null) break;

//                                lock (groupLock)
//                                {
//                                    // Create the group
//                                    groups.TryAdd(groupId, new List<IWebSocketConnection>());

//                                    foreach (var userId in members)
//                                    {
//                                        if (userId != null && onlineUsers.TryGetValue(userId, out var memberSocket))
//                                        {
//                                            if (!groups[groupId].Contains(memberSocket))
//                                            {
//                                                groups[groupId].Add(memberSocket);
//                                            }
//                                        }
//                                    }
//                                }
//                                // io.to(groupId).emit("newGroupCreated", ...)
//                                BroadcastToGroup(groupId, "newGroupCreated", payload);
//                                break;
//                            }

//                        // Replaces socket.on('addReaction', ...)
//                        case "addReaction":
//                            {
//                                var receiverId = payload.GetProperty("receiverId").GetString();
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();
//                                var chatID = payload.GetProperty("chatID").GetString();

//                                if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                {
//                                    Send(receiverSocket, "newReaction", payload);
//                                }
//                                else if (!isGroup)
//                                {
//                                    Send(socket, "userOffline", new { receiverId });
//                                }

//                                if (isGroup && chatID != null)
//                                {
//                                    BroadcastToGroup(chatID, "newReaction", payload);
//                                }
//                                break;
//                            }

//                        // Replaces socket.on('forwardMessage', ...)
//                        case "forwardMessage":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();

//                                if (isGroup)
//                                {
//                                    var groupId = payload.GetProperty("groupId").GetString();
//                                    BroadcastToGroup(groupId, "getGroupMessage", payload, excludeSocket: socket);
//                                }
//                                else
//                                {
//                                    var receiverId = payload.GetProperty("receiverId").GetString();
//                                    if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                    {
//                                        Send(receiverSocket, "getMessage", payload);
//                                    }
//                                    else
//                                    {
//                                        Send(socket, "userOffline", new { receiverId });
//                                    }
//                                }
//                                break;
//                            }

//                        // Replaces socket.on('messageSeen', ...)
//                        case "messageSeen":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();

//                                if (isGroup)
//                                {
//                                    var groupId = payload.GetProperty("groupId").GetString();
//                                    BroadcastToGroup(groupId, "updateMessageSeen", payload, excludeSocket: socket);
//                                }
//                                else
//                                {
//                                    var senderId = payload.GetProperty("senderId").GetString();
//                                    if (onlineUsers.TryGetValue(senderId, out var senderSocket))
//                                    {
//                                        Send(senderSocket, "updateMessageSeen", payload);
//                                    }
//                                }
//                                break;
//                            }


//                        // --- NEW FEATURES ---

//                        case "startTyping":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();
//                                var senderId = payload.GetProperty("senderId").GetString();
//                                var typingPayload = new { senderId, groupId = (string?)null };

//                                if (isGroup)
//                                {
//                                    var groupId = payload.GetProperty("groupId").GetString();
//                                    typingPayload = new { senderId, groupId };
//                                    BroadcastToGroup(groupId, "userTyping", typingPayload, excludeSocket: socket);
//                                }
//                                else
//                                {
//                                    var receiverId = payload.GetProperty("receiverId").GetString();
//                                    if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                    {
//                                        Send(receiverSocket, "userTyping", typingPayload);
//                                    }
//                                }
//                                break;
//                            }

//                        case "stopTyping":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();
//                                var senderId = payload.GetProperty("senderId").GetString();
//                                var typingPayload = new { senderId, groupId = (string?)null };

//                                if (isGroup)
//                                {
//                                    var groupId = payload.GetProperty("groupId").GetString();
//                                    typingPayload = new { senderId, groupId };
//                                    BroadcastToGroup(groupId, "userStoppedTyping", typingPayload, excludeSocket: socket);
//                                }
//                                else
//                                {
//                                    var receiverId = payload.GetProperty("receiverId").GetString();
//                                    if (onlineUsers.TryGetValue(receiverId, out var receiverSocket))
//                                    {
//                                        Send(receiverSocket, "userStoppedTyping", typingPayload);
//                                    }
//                                }
//                                break;
//                            }

//                        case "editMessage":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();
//                                var chatId = payload.GetProperty("chatId").GetString();

//                                if (isGroup)
//                                {
//                                    // Broadcast to everyone in the group
//                                    BroadcastToGroup(chatId, "messageEdited", payload);
//                                }
//                                else
//                                {
//                                    // Send to the receiver
//                                    if (onlineUsers.TryGetValue(chatId, out var receiverSocket))
//                                    {
//                                        Send(receiverSocket, "messageEdited", payload);
//                                    }
//                                    // Also send back to the sender, so their UI updates
//                                    Send(socket, "messageEdited", payload);
//                                }
//                                break;
//                            }

//                        case "deleteMessage":
//                            {
//                                var isGroup = payload.GetProperty("isGroup").GetBoolean();
//                                var chatId = payload.GetProperty("chatId").GetString();

//                                if (isGroup)
//                                {
//                                    BroadcastToGroup(chatId, "messageDeleted", payload);
//                                }
//                                else
//                                {
//                                    if (onlineUsers.TryGetValue(chatId, out var receiverSocket))
//                                    {
//                                        Send(receiverSocket, "messageDeleted", payload);
//                                    }
//                                    Send(socket, "messageDeleted", payload);
//                                }
//                                break;
//                            }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error processing message: {ex.Message}");
//                    Console.WriteLine($"Raw message: {message}");
//                }
//            };
//        });

//        Console.WriteLine($"Server started on ws://0.0.0.0:{port}");
//        Console.ReadLine(); // Keep the server running
//    }

//    // --- 6. Helper Methods for Sending Messages ---

//    // Send a structured message to a single socket
//    static void Send(IWebSocketConnection socket, string type, object payload)
//    {
//        var messageObject = new { type, payload };
//        var jsonMessage = JsonSerializer.Serialize(messageObject);
//        socket.Send(jsonMessage);
//    }

//    // Broadcast a message to ALL connected sockets
//    static void Broadcast(object messageObject)
//    {
//        var jsonMessage = JsonSerializer.Serialize(messageObject);
//        List<IWebSocketConnection> socketsCopy;
//        lock (socketLock)
//        {
//            socketsCopy = new List<IWebSocketConnection>(allSockets);
//        }

//        foreach (var s in socketsCopy)
//        {
//            s.Send(jsonMessage);
//        }
//    }

//    // Broadcast to a specific group
//    static void BroadcastToGroup(string groupId, string type, object payload, IWebSocketConnection? excludeSocket = null)
//    {
//        if (string.IsNullOrEmpty(groupId)) return;

//        var messageObject = new { type, payload };
//        var jsonMessage = JsonSerializer.Serialize(messageObject);

//        List<IWebSocketConnection> groupSocketsCopy = new();
//        lock (groupLock)
//        {
//            if (groups.TryGetValue(groupId, out var groupList))
//            {
//                groupSocketsCopy.AddRange(groupList);
//            }
//        }

//        foreach (var s in groupSocketsCopy)
//        {
//            if (s != excludeSocket)
//            {
//                s.Send(jsonMessage);
//            }
//        }
//    }
//}


// Program.cs
using Fleck;

class Program
{
    static void Main()
    {
        Console.WriteLine("Starting Fleck server...");

        // --- 1. Create our core services ---
        // The StateService is a singleton that manages all user/group state
        var stateService = new WebSocketStateService();
        // The Broadcaster is a singleton for sending messages
        var broadcaster = new MessageBroadcaster(stateService);

        // --- 2. Create the API/Database service (for future use) ---
        // When you're ready, you'll pass a real repository here.
        var messageRepository = new Api.MessageApiService();

        // --- 3. Create the Handler Factory ---
        // This factory knows what logic to run for each "type" string
        var handlerFactory = new MessageHandlerFactory(stateService, broadcaster, messageRepository);

        // --- 4. Create the main WebSocket Handler ---
        // This class hooks into Fleck's events
        var wsHandler = new Handlers.WebSocketHandler(stateService, broadcaster, handlerFactory);

        // --- 5. Start the Server ---
        var port = Environment.GetEnvironmentVariable("PORT") ?? "2000";
        var server = new WebSocketServer($"ws://0.0.0.0:{port}");

        // server.Start takes a *function* that receives a socket.
        // We point it to our handler's "OnNewConnection" method.
        server.Start(socket => wsHandler.OnNewConnection(socket));

        Console.WriteLine($"Server started on ws://0.0.0.0:{port}");
        // Console.ReadLine(); // Keep the server running
        // 🛑 CRITICAL FIX: Replace Console.ReadLine() with this loop
        while (true)
        {
            Thread.Sleep(Timeout.Infinite);
        }
    }
}