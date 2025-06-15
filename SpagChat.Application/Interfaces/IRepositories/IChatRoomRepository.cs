using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IChatRoomRepository
    {
        Task<ChatRoom?> CreateChatRoomAsync(ChatRoom chatRoom);
        Task<List<ChatRoom>> GetChatRoomRelatedToUserAsync(Guid UserId);
        Task<ChatRoom?> GetChatRoomByNameAsync(string chatRoomName);
        Task<ChatRoom?> GetChatRoomByIdAsync(Guid chatRoomId);
        Task<ChatRoom?> GetPrivateChatRoomAsync(List<Guid> memberIds);
        Task<bool> DeleteChatRoom(Guid chatRoomId);
        Task<bool> ChatRoomExistsAsync(Guid chatRoomId);
        Task<bool> UpdateChatRoomName(Guid ChatRoomId, string newName);
    }
}
