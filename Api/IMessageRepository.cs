// Api/IMessageRepository.cs
using Models;

namespace Api
{
    // This is the interface your API/Database class will implement
    public interface IMessageRepository
    {
        Task<ChatMessageResponse> SaveMessageAsync(ChatMessageRequest message, string authToken);

        Task<ChatMessageRequest> UpdateMessageAsync(ChatMessageRequest messageUpdate, string authToken);

        Task<ReactionResponse> AddReactionAsync(ReactionRequest reaction, string authToken);
    }
}