using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.DTO.Users;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IChatRoomUserRepository
    {
        Task<bool> AddUserToChatRoomAsync(Guid chatroomId,List<Guid> usersIds);
        Task<List<Guid>> GetChatRoomIdsByUserIdAsync(Guid userId);
        Task<List<Guid>> GetPrivateChatRoomIdsByUserIdAsync(Guid userId);
        Task<List<Guid>> GetUserIdsInChatRoomsExceptAsync(List<Guid> chatRoomIds, Guid excludeUserId);
        Task<List<ApplicationUser>> GetUsersExcludingAsync(List<Guid> excludedUserIds);
        Task<bool> RemoveUserFromChatRoomAsync(RemoveUserFromChatRoomDto userdetails);
        Task<IEnumerable<ApplicationUser>?> GetUsersFromChatRoomAsync(Guid chatroomId);
    }
}
