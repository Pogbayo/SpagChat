using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Interfaces.ICache;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Result;

namespace SpagChat.Application.Services
{
    public class ChatRoomUserService : IChatRoomUserService
    {
        private readonly ILogger<ChatRoomUserService> _logger;
        private readonly IChatRoomUserRepository _chatRoomUserRepository;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IMapper _mapper;
        private readonly ICustomMemoryCache _cache;

        public ChatRoomUserService(
            ICustomMemoryCache cache,
            IChatRoomRepository chatroomRepository,
            IMapper mapper,
            ILogger<ChatRoomUserService> logger,
            IChatRoomUserRepository chatRoomUserRepository)
        {
            _cache = cache;
            _logger = logger;
            _chatRoomRepository = chatroomRepository;
            _mapper = mapper;
            _chatRoomUserRepository = chatRoomUserRepository;
        }

        public async Task<Result<string>> AddUsersToChatRoomAsync(Guid chatroomId, List<Guid> userIds)
        {
            if (chatroomId == Guid.Empty)
            {
                _logger.LogError("Please provide a chatroom ID.");
                return Result<string>.FailureResponse("Please provide a valid chatroom ID.");
            }

            if (userIds == null || !userIds.Any())
            {
                _logger.LogError("User list is empty.");
                return Result<string>.FailureResponse("User list cannot be empty.");
            }

            var chatRoom = await _chatRoomRepository.GetChatRoomByIdAsync(chatroomId);

            if (chatRoom == null)
            {
                _logger.LogError("Chatroom not found.");
                return Result<string>.FailureResponse("Chatroom not found.");
            }

            if (!chatRoom.IsGroup)
            {
                _logger.LogWarning("Private chat can only contain two users");
                return Result<string>.FailureResponse("Private chat can only contain two users");
            }

            var existingUserIds = chatRoom.ChatRoomUsers?.Select(u => u.UserId).ToHashSet() ?? new HashSet<Guid>();

            foreach (var userId in userIds)
            {
                if (existingUserIds.Contains(userId))
                {
                    _logger.LogError("User can not exist in a group twice");
                    return Result<string>.FailureResponse("User is already in this group");
                }
            }

            //foreach (var userId in userIds)
            //{
            //    var chatRooms = await _chatRoomRepository.GetChatRoomRelatedToUserAsync(userId);

            //    foreach (var ch in chatRooms)
            //    {
            //        if (ch.ChatRoomId == chatroomId)
            //        {
            //            _logger.LogError("User can not exist in a group twice");
            //            return Result<string>.FailureResponse("User can not exist in a group twice");
            //        }
            //    }
            //}

            var result = await _chatRoomUserRepository.AddUserToChatRoomAsync(chatroomId, userIds);
            if (!result)
            {
                _logger.LogError("Failed to add users to the chat room.");
                return Result<string>.FailureResponse("Failed to add users to the chat room.");
            }

            _cache.RemoveByPrefix("GetUsersFromChatRoom_");
            return Result<string>.SuccessResponse("User{s} added to chat room successfully.");
        }

        public async Task<Result<IEnumerable<ApplicationUserDto>>> GetUsersFromChatRoomAsync(Guid chatroomId)
        {
            if (chatroomId == Guid.Empty)
            {
                _logger.LogError("Please provide a chatroom ID.");
                return Result<IEnumerable<ApplicationUserDto>>.FailureResponse("Please provide a valid chatroom ID.");
            }

            string cacheKey = $"GetUsersFromChatRoom_{chatroomId}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<ApplicationUserDto> cachedUserList))
            {
                _logger.LogInformation($"Returning users from cache for ChatRoom ID: {chatroomId}");
                return Result<IEnumerable<ApplicationUserDto>>.SuccessResponse(cachedUserList);
            }

            var chatRoom = await _chatRoomRepository.GetChatRoomByIdAsync(chatroomId);
            if (chatRoom == null)
            {
                _logger.LogError("Chat room with provided ID does not exist.");
                return Result<IEnumerable<ApplicationUserDto>>.FailureResponse("Chat room not found.");
            }

            var users = await _chatRoomUserRepository.GetUsersFromChatRoomAsync(chatroomId);
            var mappedResult = _mapper.Map<IEnumerable<ApplicationUserDto>>(users);

            _cache.Set(cacheKey, mappedResult, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

            _logger.LogInformation($"Caching users for chat room: {chatRoom.Name}");

            return Result<IEnumerable<ApplicationUserDto>>.SuccessResponse(mappedResult,$"Users in the {chatRoom.Name} group");
        }

        public async Task<Result<List<ApplicationUserDto>>> GetNonMutualFriendsAsync(Guid currentUserId)
        {
            _logger.LogInformation("Fetching non-mutual (1-on-1) friends for user: {UserId}", currentUserId);

            if (currentUserId == Guid.Empty)
            {
                return Result<List<ApplicationUserDto>>.FailureResponse("Invalid user ID.");
            }

            try
            {
                var privateChatRoomIds = await _chatRoomUserRepository.GetPrivateChatRoomIdsByUserIdAsync(currentUserId);

                var mutualUserIds = await _chatRoomUserRepository.GetUserIdsInChatRoomsExceptAsync(privateChatRoomIds, currentUserId);

                var nonMutualUsers = await _chatRoomUserRepository.GetUsersExcludingAsync(mutualUserIds.Append(currentUserId).ToList());

                var mapped = _mapper.Map<List<ApplicationUserDto>>(nonMutualUsers);

                _logger.LogInformation("Found {Count} non-mutual users (1-on-1) for user: {UserId}", mapped.Count, currentUserId);

                return Result<List<ApplicationUserDto>>.SuccessResponse(mapped, "Users without private chats fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch non-mutual friends.");
                return Result<List<ApplicationUserDto>>.FailureResponse("Failed to fetch non-mutual friends.", ex.Message);
            }
        }




        public async Task<Result<string>> RemoveUserFromChatRoomAsync(RemoveUserFromChatRoomDto userDetails)
        {
            if (userDetails.UserId == Guid.Empty || userDetails.ChatRoomId == Guid.Empty)
            {
                _logger.LogError("Invalid IDs provided.");
                return Result<string>.FailureResponse("Invalid user or chat room user ID.");
            }

            var result = await _chatRoomUserRepository.RemoveUserFromChatRoomAsync(userDetails);
            if (!result)
            {
                _logger.LogError("Failed to remove user from chat room.");
                return Result<string>.FailureResponse("Failed to remove user from chat room.");
            }

            return Result<string>.SuccessResponse("User removed from chat room successfully.","User successfully removed");
        }

    }
}
