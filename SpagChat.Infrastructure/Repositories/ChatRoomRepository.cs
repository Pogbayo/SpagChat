using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Domain.Entities;
using SpagChat.Infrastructure.Persistence;

namespace SpagChat.Infrastructure.Repositories
{
    public class ChatRoomRepository : IChatRoomRepository
    {
        private readonly ILogger<ChatRoomRepository> _logger;
        private readonly SpagChatDb _dbContext;

        public ChatRoomRepository(ILogger<ChatRoomRepository> logger, SpagChatDb context)
        {
            _logger = logger;
            _dbContext = context;
        }

        public async Task<bool> ChatRoomExistsAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Chat room ID cannot be empty");
                return false;
            }

            _logger.LogInformation($"Checking existence of chat room with ID: {chatRoomId}");

            bool exists = await _dbContext.ChatRooms.AnyAsync(cr => cr.ChatRoomId == chatRoomId);

            _logger.LogInformation($"Chat room with ID {chatRoomId} exists: {exists}");

            return exists;
        }

        public async Task<ChatRoom?> CreateChatRoomAsync(ChatRoom chatRoom)
        {
            _logger.LogInformation($"Creating chat room: {chatRoom.Name} with ID {chatRoom.ChatRoomId}");

            await _dbContext.ChatRooms.AddAsync(chatRoom);
            await _dbContext.SaveChangesAsync();

            var createdChatRommWithUsers = await _dbContext.ChatRooms
                .Include(c => c.ChatRoomUsers!)
                  .ThenInclude(cru => cru.User)
                .FirstOrDefaultAsync(c => c.ChatRoomId == chatRoom.ChatRoomId);

            _logger.LogInformation($"Chat room created successfully: {chatRoom.ChatRoomId}");

            return createdChatRommWithUsers;
        }

        public async Task<bool> DeleteChatRoom(Guid chatRoomId)
        {
            _logger.LogInformation($"Attempting to delete chat room with ID: {chatRoomId}");
            var chatRoomRecord = await _dbContext.ChatRooms.FindAsync(chatRoomId);
            if (chatRoomRecord == null)
            {
                _logger.LogWarning($"Chat room record with ID {chatRoomId} not found");
                return false;
            }

            _dbContext.ChatRooms.Remove(chatRoomRecord);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Chat room with ID {chatRoomId} deleted successfully");
            return true;
        }

        public async Task<ChatRoom?> GetPrivateChatRoomAsync(List<Guid> memberIds)
        {
            return await _dbContext.ChatRooms
                .Include(c => c.ChatRoomUsers)
                .FirstOrDefaultAsync(c =>
                    !c.IsGroup &&
                    c.ChatRoomUsers!.Count == 2 &&
                    c.ChatRoomUsers.All(cru => memberIds.Contains(cru.UserId)));
        }


        public async Task<ChatRoom?> GetChatRoomByIdAsync(Guid chatRoomId)
        {
            _logger.LogInformation($"Fetching chat room by name: {chatRoomId}");
            var chatRoomRecord = await _dbContext.ChatRooms
                .Include(c => c.Messages)
                .Include(c => c.ChatRoomUsers!)
                  .ThenInclude(cru => cru.User)
                .FirstOrDefaultAsync(cr => cr.ChatRoomId == chatRoomId);
            _logger.LogInformation($"Number of messages in chat room: {chatRoomRecord!.Messages?.Count}");
            if (chatRoomRecord?.Messages != null)
            {
                foreach (var msg in chatRoomRecord.Messages)
                {
                    _logger.LogInformation($"Message: {msg.Content}, Timestamp: {msg.Timestamp}");
                }
            }
            return chatRoomRecord;
        }

        public async Task<ChatRoom?> GetChatRoomByNameAsync(string chatRoomName)
        {
            _logger.LogInformation($"Fetching chat room by name: {chatRoomName}");
            var chatRoomRecord = await _dbContext.ChatRooms
                .Include(c => c.Messages)
                .Include(c => c.ChatRoomUsers!)
                  .ThenInclude(cru => cru.User)
                .FirstOrDefaultAsync(cr => cr.Name == chatRoomName);
            if (chatRoomRecord == null)
            {
                _logger.LogWarning($"Chat room with name '{chatRoomName}' not found.");
            }
            else
            {
                _logger.LogInformation($"Chat room found: {chatRoomRecord.Name}");
            }
            return chatRoomRecord;
        }

        public async Task<List<ChatRoom>> GetChatRoomRelatedToUserAsync(Guid UserId)
        {
            _logger.LogInformation($"Fetching chat rooms for user ID: {UserId}");
            var chatRooms = await _dbContext.ChatRooms
                .Where(cr => cr.ChatRoomUsers != null && cr.ChatRoomUsers.Any(cu => cu.UserId == UserId))
                .Include(cr => cr.Messages)
                .Include(cr => cr.ChatRoomUsers!)
                .ThenInclude(cu => cu!.User)
                .ToListAsync();
            _logger.LogInformation($"Found {chatRooms.Count} chat rooms for user ID {UserId}");
            return chatRooms;
        }

        public async Task<bool> UpdateChatRoomName(Guid chatRoomId, string newName)
        {
            _logger.LogInformation($"Updating chat room name for ID {chatRoomId} to '{newName}'");
            var chatRoomRecord = await _dbContext.ChatRooms.FindAsync(chatRoomId);
            if (chatRoomRecord == null)
            {
                _logger.LogError($"Chat room with ID {chatRoomId} not found");
                return false;
            }
            chatRoomRecord.Name = newName;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Chat room name updated successfully for ID {chatRoomId}");
            return true;
        }

        public async Task<List<Guid>> GetChatRoomIdsForUserAsync(Guid userId)
        {
            return await _dbContext.ChatRoomUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ChatRoomId)
                .ToListAsync();
        }
    }
}
