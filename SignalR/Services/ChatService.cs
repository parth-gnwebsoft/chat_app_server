// Handlers/Logic/ChatService.cs
using Api;
using Models;

namespace Services.Logic
{
    // This class just holds the business logic for your API
    public class ChatService
    {
        private readonly IMessageRepository _messageRepository;

        public ChatService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        } 

        public Task<ChatMessageResponse> SaveMessageAsync(ChatMessageRequest message, string token)
        {
            return _messageRepository.SaveMessageAsync(message, token);
        }
         
        public Task<ChatMessageResponse> UpdateMessageAsync(ChatMessageRequest messageUpdate, string token)
        {
            return _messageRepository.UpdateMessageAsync(messageUpdate, token);
        }
    }
}