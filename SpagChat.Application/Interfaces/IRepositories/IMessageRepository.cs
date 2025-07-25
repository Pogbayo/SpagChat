﻿using SpagChat.Application.DTO.Messages;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.Interfaces.IRepositories
{
    public interface IMessageRepository
    {
        Task<Message?> SendMessageAsync(Message messageDetails);
        Task<IEnumerable<Message>?> GetMessagesByChatRoomIdAsync(Guid chatRoomId);
        Task<bool> DeleteAsync(Guid MessageId);
        Task AddMessageReadByAsync(Guid messageId, Guid userId);
        Task<bool> EditMessageAsync(Guid MessageId, string newContent);
        Task<Message> GetMessageByIdAsync(Guid messageId);
    }
}
