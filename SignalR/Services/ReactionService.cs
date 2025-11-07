// Handlers/Logic/ReactionService.cs
using Api;
using Models;

namespace Services.Logic
{
    public class ReactionService
    {
        private readonly IMessageRepository _messageRepository;

        public ReactionService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public Task<ReactionResponse> AddReactionAsync(ReactionRequest reaction, string token)
        {
            return _messageRepository.AddReactionAsync(reaction, token); 
        }
    }
}