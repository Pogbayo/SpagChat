﻿using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.Result;

namespace SpagChat.Application.Interfaces.IServices
{
    public interface IChatRoomService
    {
        Task<Result<ChatRoomDto?>> CreateChatRoomAsync(CreateChatRoomDto chatRoomDetails);
        Task<Result<List<ChatRoomDto>>> GetChatRoomsUserIsNotInAsync(Guid currentUserId);
        Task<Result<List<ChatRoomDto>>> GetChatRoomRelatedToUserAsync(Guid UserId);
        Task<Result<ChatRoomDto?>> GetChatRoomByNameAsync(string chatRoomName);
        Task<Result<ChatRoomDto?>> GetChatRoomByIdAsync(Guid chatRoomId);
        Task<Result<ChatRoomDto?>> GetPrivateChatAsync(List<Guid> memberIds);
        Task<Result<List<Guid>>> GetChatRoomIdsForUserAsync(Guid userId);
        Task<bool> IsUserInChatRoom(Guid chatRoomId, Guid userId);
        Task<Result<bool>> DeleteChatRoomAsync(Guid chatRoomId);
        Task<Result<bool>> ChatRoomExistsAsync(Guid chatRoomId);
        Task<Result<Dictionary<Guid,int>>> GetUnreadMessageCountAsync(Guid userId);
        Task<Result<bool>> MarkMessagesAsReadAsync(List<Guid> memberIds, Guid userId);
        Task<Result<bool>> UpdateChatRoomNameAsync(Guid chatRoomId, string newName);
    }
}
