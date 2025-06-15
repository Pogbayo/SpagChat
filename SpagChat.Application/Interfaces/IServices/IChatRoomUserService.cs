using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Result;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface IChatRoomUserService
    {
        Task<Result<string>> AddUsersToChatRoomAsync(Guid chatroomId, List<Guid> userIds);
        Task<Result<string>> RemoveUserFromChatRoomAsync(RemoveUserFromChatRoomDto userDetails);
        Task<Result<IEnumerable<ApplicationUserDto>>> GetUsersFromChatRoomAsync(Guid chatroomId);
    }
}
