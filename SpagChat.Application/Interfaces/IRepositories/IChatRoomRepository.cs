using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> CreateChatRoomAsync(ChatRoom chatRoom);
        Task<List<ChatRoom>> GetChatRoomsthatUserIsNotIn(Guid currentUserId);
        Task<List<ChatRoom>> GetChatRoomRelatedToUserAsync(Guid UserId);
        Task<ChatRoom?> GetChatRoomByNameAsync(string chatRoomName);
        Task<ChatRoom?> GetChatRoomWithUsersByIdAsync(Guid chatRoomId);
        Task<ChatRoom?> GetChatRoomByIdAsync(Guid chatRoomId);
        Task<ChatRoom?> GetPrivateChatRoomAsync(List<Guid> memberIds);
        Task<List<Guid>> GetChatRoomIdsForUserAsync(Guid userId);
        Task<bool> DeleteChatRoom(Guid chatRoomId);
        Task<bool> ChatRoomExistsAsync(Guid chatRoomId);
        Task<int> GetUnreadMessageCountAsync(Guid chatRoomId, Guid userId);
        //Task<List<Message>> GetUnreadMessagesAsync(Guid chatRoomId, Guid userId);
        Task MarkMessagesAsReadAsync(Guid chatRoomId, Guid userId);
        Task<bool> UpdateChatRoomName(Guid ChatRoomId, string newName);
    }
}
