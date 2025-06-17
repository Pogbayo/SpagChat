using SpagChat.Application.DTO.Messages;
using SpagChat.Application.Result;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface IMessageService
    {
        Task<Result<bool>> SendMessageAsync(SendMessageDto messageDetails);
        Task<Result<IEnumerable<MessageDto>>> GetMessagesByChatRoomIdAsync(Guid chatRoomId);
        Task<Result<bool>> DeleteAsync(Guid MessageId);
        Task<Result<bool>> EditMessageAsync(Guid MessageId, string newContent);
    }
}
