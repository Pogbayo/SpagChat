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
            _dbContext.Messages.Remove(message);
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
            message.Content = newContent;
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Message>?> GetMessagesByChatRoomIdAsync(Guid chatRoomId)
        {
            var chatRoom = await _dbContext.ChatRooms
                .Include(cr => cr.Messages!)
                 .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(cr => cr.ChatRoomId == chatRoomId);

            if (chatRoom == null)
            {
                _logger.LogError("ChatRoom does not have any message");
                return null;
            }

            _logger.LogInformation("ChatRoom messages logged out successfully");
            return chatRoom.Messages;
        }

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
