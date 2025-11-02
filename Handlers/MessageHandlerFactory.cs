// Handlers/MessageHandlerFactory.cs
using Handlers.Logic;
using Api; // For the stub repository

public class MessageHandlerFactory
{
    private readonly Dictionary<string, IMessageHandler> _handlers;

    // We use Dependency Injection to give the handlers the services they need
    public MessageHandlerFactory(
        IWebSocketStateService stateService,
        IMessageBroadcaster broadcaster,
        IMessageRepository messageRepository // Your future API service
        )
    {
        _handlers = new Dictionary<string, IMessageHandler>
        {
            { "addUser", new AddUserHandler(stateService, broadcaster) },
            { "sendMessage", new SendMessageHandler(stateService, broadcaster, messageRepository) },//
            { "sendReply", new SendReplyHandler(stateService, broadcaster, messageRepository) },//
            { "sendGroupMessage", new SendGroupMessageHandler(broadcaster, messageRepository) },
            { "forwardMessage", new ForwardMessageHandler(stateService, broadcaster, messageRepository) },//
            { "messageSeen", new MessageSeenHandler(stateService, broadcaster, messageRepository) },
            { "editMessage", new EditMessageHandler(stateService, broadcaster, messageRepository) },
            { "deleteMessage", new DeleteMessageHandler(stateService, broadcaster, messageRepository) },
            { "addReaction", new AddReactionHandler(stateService, broadcaster, messageRepository) },

            { "joinGroup", new JoinGroupHandler(stateService) },
            { "leaveGroup", new LeaveGroupHandler(stateService) },
            { "createGroup", new CreateGroupHandler(stateService, broadcaster) },
            { "startTyping", new StartTypingHandler(stateService, broadcaster) },
            { "stopTyping", new StopTypingHandler(stateService, broadcaster) },
        };
    }

    public IMessageHandler? GetHandler(string messageType)
    {
        _handlers.TryGetValue(messageType, out var handler);
        return handler;
    }
}