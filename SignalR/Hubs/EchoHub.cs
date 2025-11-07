// Hubs/EchoHub.cs

using Api;
using Microsoft.AspNetCore.SignalR;
using Models;
using Services.Logic;

using WebSocketTest.Services; // Your ConnectionTracker

namespace WebSocketTest.Hubs;

public class EchoHub : Hub
{
    private readonly ConnectionTracker _connectionTracker;
    private readonly ChatService _chatService;
    private readonly ReactionService _reactionService;
    private readonly IMessageRepository _messageRepository;

    // 1. Inject all required services
    public EchoHub(
        ConnectionTracker connectionTracker,
        ChatService chatService,
                ReactionService reactionService,
        IMessageRepository messageRepository) 
    {
        _connectionTracker = connectionTracker;
        _chatService = chatService;
        _reactionService = reactionService;
        _messageRepository = messageRepository; 
    }


    #region OnConnectedAsync
    public override async Task OnConnectedAsync( )
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext.Request.Query["userId"].ToString();
        var token = httpContext.Request.Query["token"].ToString(); // <-- Get the token

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            Context.Abort();
            return;
        }

        // --- Call API to set status to TRUE ---
        try
        {
            var statusRequest = new UpdateOnlineStatusRequest
            {
                UserID = int.Parse(userId),
                IsOnlineInChat = true
            };
            await _messageRepository.UpdateUserOnlineStatusAsync(statusRequest, token);
        }
        catch (Exception ex)
        {
            // Log the error but still allow connection
            Console.WriteLine($"Failed to set user online status during connect: {ex.Message}");
        }

        var connectionId = Context.ConnectionId;
        _connectionTracker.Add(connectionId, userId, token); // <-- Store the token

        await Clients.Caller.SendAsync("ReceiveMessage",
            $"[System] Connected! Your Connection ID: {connectionId}");

        var userCount = _connectionTracker.GetCount();
        await Clients.All.SendAsync("ReceiveUserCount", userCount);
        await BroadcastClientList();

        await base.OnConnectedAsync();
    }
    #endregion

    #region OnDisconnectedAsync
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // --- Find connection info BEFORE removing it ---
        var connectionInfo = _connectionTracker.GetConnection(Context.ConnectionId);

        if (connectionInfo != null)
        {
            // --- Call API to set status to FALSE ---
            try
            {
                var statusRequest = new UpdateOnlineStatusRequest
                { 
                    UserID = int.Parse(connectionInfo.UserId),
                    IsOnlineInChat = false
                };
                // Use the token we stored in the tracker
                await _messageRepository.UpdateUserOnlineStatusAsync(statusRequest, connectionInfo.AuthToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set user offline status during disconnect: {ex.Message}");
            }
        }

        // --- Now, remove from tracker ---
        _connectionTracker.Remove(Context.ConnectionId);

        var userCount = _connectionTracker.GetCount();
        await Clients.All.SendAsync("ReceiveUserCount", userCount);
        await BroadcastClientList();

        await base.OnDisconnectedAsync(exception);
    }
    #endregion

    #region joinGroup
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("ReceiveMessage", $"[System] You joined group: {groupName}");
    }
    #endregion

    #region leaveGroup
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.SendAsync("ReceiveMessage", $"[System] You left group: {groupName}");
    }
    #endregion

    #region SendMessage
    public async Task SendMessage(string token, ChatMessageRequest message)
    {
        try
        {
            ChatMessageResponse savedMessage = await _chatService.SaveMessageAsync(message, token);

            if (savedMessage.ChannelType == "Group")
            {
                // Send to group (this includes the sender)
                savedMessage.mobileMessageID = message.mobileMessageID; // Echo back mobileMessageID
                await Clients.Group(savedMessage.ChannelID.ToString())
                             .SendAsync("ReceiveMessage", savedMessage);
            }
            else
            {
                // Send to self (sender)
                savedMessage.mobileMessageID= message.mobileMessageID; // Echo back mobileMessageID
                await Clients.Caller.SendAsync("ReceiveMessage", savedMessage);

                // Find and send to receiver
                var receiverId = savedMessage.ReciverUserID.ToString();
                var receiverConnection = _connectionTracker.GetConnectionByUserId(receiverId);
                if (receiverConnection != null)
                {
                    await Clients.Client(receiverConnection.ConnectionId)
                                 .SendAsync("ReceiveMessage", savedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Failed to send message: {ex.Message}");
        }
    }

    #endregion

    #region EditMessage
    public async Task EditMessage(string token, ChatMessageRequest messageUpdate)
    {
        try
        {
            ChatMessageResponse updatedMessage = await _chatService.UpdateMessageAsync(messageUpdate, token);

            if (updatedMessage.ChannelType == "Group")
            {
                await Clients.Group(updatedMessage.ChannelID.ToString())
                             .SendAsync("ReceiveMessageEdited", updatedMessage);
            }
            else
            {
                // Send to self
                await Clients.Caller.SendAsync("ReceiveMessageEdited", updatedMessage);

                // Find and send to receiver
                var receiverId = updatedMessage.ReciverUserID.ToString();
                var receiverConnection = _connectionTracker.GetConnectionByUserId(receiverId);
                if (receiverConnection != null)
                {
                    await Clients.Client(receiverConnection.ConnectionId)
                                 .SendAsync("ReceiveMessageEdited", updatedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Failed to update message: {ex.Message}");
        }
    }
    #endregion

    #region deleteMessage
    public async Task DeleteMessage(string token, ChatMessageRequest messageUpdate)
    {
        try
        {
            ChatMessageResponse updatedMessage = await _chatService.UpdateMessageAsync(messageUpdate, token);

            if (updatedMessage.ChannelType == "Group")
            {
                await Clients.Group(updatedMessage.ChannelID.ToString())
                             .SendAsync("ReceiveMessageDeleted", updatedMessage);
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessageDeleted", updatedMessage);
                var receiverId = updatedMessage.ReciverUserID.ToString();
                var receiverConnection = _connectionTracker.GetConnectionByUserId(receiverId);
                if (receiverConnection != null)
                {
                    await Clients.Client(receiverConnection.ConnectionId)
                                 .SendAsync("ReceiveMessageDeleted", updatedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Failed to delete message: {ex.Message}");
        }
    }
    #endregion

    #region AddReaction
    public async Task AddReaction(string token, ReactionRequest reaction, bool isGroup, string chatOrUserId)
    {
        try
        {
            ReactionResponse savedReaction = await _reactionService.AddReactionAsync(reaction, token);

            if (isGroup)
            {
                // chatOrUserId is the groupName/ChannelID
                await Clients.Group(chatOrUserId).SendAsync("ReceiveReaction", savedReaction);
            }
            else
            {
                // chatOrUserId is the receiver's UserId
                await Clients.Caller.SendAsync("ReceiveReaction", savedReaction);

                var receiverConnection = _connectionTracker.GetConnectionByUserId(chatOrUserId);
                if (receiverConnection != null)
                {
                    await Clients.Client(receiverConnection.ConnectionId)
                                 .SendAsync("ReceiveReaction", savedReaction);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Failed to add reaction: {ex.Message}");
        }
    }
    #endregion

    #region StartTyping
    public async Task StartTyping(string chatOrUserId, bool isGroup)
    {
        var sender = _connectionTracker.GetConnection(Context.ConnectionId);
        if (sender == null) return;

        var payload = new { senderId = sender.UserId, chatId = chatOrUserId };

        if (isGroup)
        {
            // Broadcast to all *other* members of the group
            await Clients.OthersInGroup(chatOrUserId).SendAsync("ReceiveStartTyping", payload);
        }
        else
        {
            // Send to the specific user
            var receiverConnection = _connectionTracker.GetConnectionByUserId(chatOrUserId);
            if (receiverConnection != null)
            {
                await Clients.Client(receiverConnection.ConnectionId).SendAsync("ReceiveStartTyping", payload);
            }
        }
    }
    #endregion

    #region StopTyping
    public async Task StopTyping(string chatOrUserId, bool isGroup)
    {
        var sender = _connectionTracker.GetConnection(Context.ConnectionId);
        if (sender == null) return;

        var payload = new { senderId = sender.UserId, chatId = chatOrUserId };

        if (isGroup)
        {
            await Clients.OthersInGroup(chatOrUserId).SendAsync("ReceiveStopTyping", payload);
        }
        else
        {
            var receiverConnection = _connectionTracker.GetConnectionByUserId(chatOrUserId);
            if (receiverConnection != null)
            {
                await Clients.Client(receiverConnection.ConnectionId).SendAsync("ReceiveStopTyping", payload);
            }
        }
    }
    #endregion

    #region MessageSeen
    public async Task MessageSeen(string token, ChatMessageRequest messageUpdate)
    {
        try
        {
            // "MessageSeen" is just an "Update" call, so we use the same service
            ChatMessageResponse updatedMessage = await _chatService.UpdateMessageAsync(messageUpdate, token);

            if (updatedMessage.ChannelType == "Group")
            {
                await Clients.Group(updatedMessage.ChannelID.ToString())
                             .SendAsync("ReceiveMessageSeen", updatedMessage);
            }
            else
            {
                // Send update to self (the person who saw the message)
                await Clients.Caller.SendAsync("ReceiveMessageSeen", updatedMessage);

                // Send update to the original sender
                var receiverId = updatedMessage.ReciverUserID.ToString();
                var receiverConnection = _connectionTracker.GetConnectionByUserId(receiverId);
                if (receiverConnection != null)
                {
                    await Clients.Client(receiverConnection.ConnectionId).SendAsync("ReceiveMessageSeen", updatedMessage);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"Failed to mark message as seen: {ex.Message}");
        }
    }
    #endregion

    #region CreateGroup
    public async Task CreateGroup(string groupId, string groupName, List<string> memberUserIds)
    {
        // The caller (group creator) joins the group
        await JoinGroup(groupId);
         
        // Tell all other members to join the group
        foreach (var userId in memberUserIds)
        {
            var memberConnection = _connectionTracker.GetConnectionByUserId(userId);
            if (memberConnection != null)
            {
                // This event tells the client-side JS to call its own "join" logic
                await Clients.Client(memberConnection.ConnectionId)
                             .SendAsync("ReceiveNewGroup", new { groupId, groupName });
            }
        }
    }
    #endregion

    #region GetConnectedClients
    public IEnumerable<ClientConnectionInfo> GetConnectedClients()
    {
        return _connectionTracker.GetAllConnections();
    }
    #endregion

    #region BroadcastClientList
    private async Task BroadcastClientList()
    {
        var clients = _connectionTracker.GetAllConnections();
        await Clients.All.SendAsync("ReceiveClientList", clients);
    }
    #endregion
}