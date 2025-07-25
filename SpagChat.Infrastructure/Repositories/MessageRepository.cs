using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Domain.Entities;
using SpagChat.Infrastructure.Persistence;

namespace SpagChat.Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ILogger<MessageRepository> _logger;
        private readonly SpagChatDb _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public MessageRepository(ILogger<MessageRepository> logger, SpagChatDb context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _dbContext = context;
            _userManager = userManager;
        }
        public async Task<bool> DeleteAsync(Guid MessageId)
        {
            if (MessageId == Guid.Empty)
            {
                _logger.LogError("Message Id is empty");
                return false;
            }
            var message = await _dbContext.Messages.FindAsync(MessageId);
            if (message == null)
            {
                _logger.LogWarning("Message with id does not exist");
                return false;
            }
             message.IsDeleted = true;
            //_dbContext.Messages.Remove(message);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
        public async Task<bool> EditMessageAsync(Guid MessageId, string newContent)
        {
            if (MessageId == Guid.Empty)
            {
                _logger.LogError("Message Id is empty");
                return false;
            }
            var message = await _dbContext.Messages.FindAsync(MessageId);
            if (message == null)
            {
                _logger.LogWarning("Message with id does not exist");
                return false;
            }
            message.IsEdited = true;
            message.Content = newContent;
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
        public async Task<Message> GetMessageByIdAsync(Guid messageId)
        {
            var message = await _dbContext.Messages.FindAsync(messageId);            //if (message != null) 
            return message!;
        }
        public async Task AddMessageReadByAsync(Guid messageId, Guid userId)
        {
            var messageReadBy = new MessageReadBy
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            };
            _dbContext.MessageReadBy.Add(messageReadBy);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<Message>?> GetMessagesByChatRoomIdAsync(Guid chatRoomId)
        {
            _logger.LogInformation("Heloeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee");
            var messages = await _dbContext.Messages
              .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
              .Include(m => m.Sender)
              .Include(m => m.ReadBy)
              .OrderBy(m => m.Timestamp)
              .ToListAsync();

            _logger.LogInformation($"Found {messages.Count} messages for chatRoom {chatRoomId}");
            return messages;
        }

        //public async Task<IEnumerable<Message>?> GetMessagesByChatRoomIdAsync(Guid chatRoomId)
        //{
        //    _logger.LogInformation($"Searching for ChatRoomId: {chatRoomId} (ToString: '{chatRoomId}')");

        //    // Log the total count first
        //    var totalMessages = await _dbContext.Messages
        //        .Where(m => m.ChatRoomId == chatRoomId)
        //        .CountAsync();
        //    _logger.LogInformation($"Total messages for chatRoom {chatRoomId}: {totalMessages}");

        //    // Log how many are deleted
        //    var deletedMessages = await _dbContext.Messages
        //        .Where(m => m.ChatRoomId == chatRoomId && m.IsDeleted)
        //        .CountAsync();
        //    _logger.LogInformation($"Deleted messages for chatRoom {chatRoomId}: {deletedMessages}");

        //    // Your original query
        //    var messages = await _dbContext.Messages
        //        .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
        //        .Include(m => m.Sender)
        //        .Include(m => m.ReadBy)
        //        .OrderBy(m => m.Timestamp)
        //        .ToListAsync();

        //    _logger.LogInformation($"Final result: {messages.Count} messages");

        //    if (!messages.Any())
        //    {
        //        _logger.LogError("ChatRoom does not have any message");
        //    }
        //    else
        //    {
        //        _logger.LogInformation("ChatRoom messages logged out successfully");
        //    }

        //    return messages;
        //}

        public async Task<Message?> SendMessageAsync(Message messageDetails)
        {
            var chatRoom = await _dbContext.ChatRooms.FindAsync(messageDetails.ChatRoomId);

            if (chatRoom == null)
            {
                _logger.LogWarning($"Chat room with ID {messageDetails.ChatRoomId} not found.");
                return null;
            }

            var sender = await _userManager.FindByIdAsync(messageDetails.SenderId.ToString());

            if (sender == null)
            {
                _logger.LogWarning($"User with ID {messageDetails.SenderId} not found.");
                return null;
            }

            var message = new Message
            {
                Content = messageDetails.Content,
                ChatRoomId = chatRoom.ChatRoomId,
                SenderId = messageDetails.SenderId,
                Timestamp = DateTime.UtcNow
            };

            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }
    }
}
