using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Domain.Entities;
using SpagChat.Infrastructure.Persistence;


namespace SpagChat.Infrastructure.Repositories
{
    public class ChatRoomUserRepository : IChatRoomUserRepository
    {
        private readonly ILogger<ChatRoomUserRepository> _logger;
        private readonly SpagChatDb _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatRoomUserRepository(UserManager<ApplicationUser> userManager,ILogger<ChatRoomUserRepository> logger, SpagChatDb dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<bool> AddUserToChatRoomAsync(Guid chatroomId, List<Guid> usersIdList)
        {
            bool chatRoomExists = await _dbContext.ChatRooms
                .AsNoTracking()
                .AnyAsync(cr => cr.ChatRoomId == chatroomId);

            if (!chatRoomExists)
            {
                _logger.LogWarning($"Chat room with ID {chatroomId} not found.");
                return false;
            }


            foreach (var userId in usersIdList)
            {
                var chatRoomUser = new ChatRoomUser
                {
                    ChatRoomUserId = Guid.NewGuid(), 
                    ChatRoomId = chatroomId,
                    UserId = userId
                };

                await _dbContext.ChatRoomUsers.AddAsync(chatRoomUser);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while adding users to the chat room.");
                return false;
            }
        }

        public async Task<List<Guid>> GetChatRoomIdsByUserIdAsync(Guid userId)
        {
            return await _dbContext.ChatRoomUsers
                .Where(cu => cu.UserId == userId)
                .Select(cu => cu.ChatRoomId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Guid>> GetUserIdsInChatRoomsExceptAsync(List<Guid> chatRoomIds, Guid excludeUserId)
        {
            return await _dbContext.ChatRoomUsers
                .Where(cu => chatRoomIds.Contains(cu.ChatRoomId) && cu.UserId != excludeUserId)
                .Select(cu => cu.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Guid>> GetPrivateChatRoomIdsByUserIdAsync(Guid userId)
        {
            return await _dbContext.ChatRoomUsers
                .Where(cu => cu.UserId == userId && cu.ChatRoom.IsGroup == false)
                .Select(cu => cu.ChatRoomId)
                .ToListAsync();
        }


        public async Task<List<ApplicationUser>> GetUsersExcludingAsync(List<Guid> excludedUserIds)
        {
            return await _dbContext.Users
                .Where(u => !excludedUserIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>?> GetUsersFromChatRoomAsync(Guid chatroomId)
        {
            var chatRoom = await _dbContext.ChatRooms
                .Where(cr => cr.ChatRoomId == chatroomId)
                .Include(cr => cr.ChatRoomUsers!)
                   .ThenInclude(cu => cu.User)
                 .FirstOrDefaultAsync();

            if (chatRoom == null)
            {
                _logger.LogWarning($"Chat room with ID {chatroomId} not found.");
                return Enumerable.Empty<ApplicationUser>();
            }

            return chatRoom.ChatRoomUsers!.Select(cu => cu.User) ;
        }

        public async Task<bool> RemoveUserFromChatRoomAsync(RemoveUserFromChatRoomDto userdetails)
        {
            var chatRoom = await _dbContext.ChatRooms
                            .Where(cr => cr.ChatRoomId == userdetails.ChatRoomId)
                            .Include(cr => cr.ChatRoomUsers!)
                            .FirstOrDefaultAsync();

            if (chatRoom == null)
            {
                _logger.LogWarning($"Chat room with ID {userdetails.ChatRoomId} not found.");
                return false;
            }
            if (chatRoom.ChatRoomUsers == null)
            {
                _logger.LogError("ChatRoom is emoty");
                return false;
            }

            var userToRemove = await _userManager.FindByIdAsync(userdetails.UserId.ToString());

            if (userToRemove == null)
            {
                _logger.LogWarning($"User with provided Id: {userdetails.UserId} does not exist");
                return false;
            }

            var chatRoomUser = chatRoom.ChatRoomUsers.FirstOrDefault(u => u.UserId == userdetails.UserId);

            if (chatRoomUser == null)
            {
                _logger.LogWarning($"User with ID {userdetails.UserId} is not in chat room {userdetails.ChatRoomId}.");
                return false;
            }

            chatRoom.ChatRoomUsers.Remove(chatRoomUser);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
    }
}
