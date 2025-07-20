using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Interfaces.ICache;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Result;
using SpagChat.Domain.Entities;
using System.Text.Json;

namespace SpagChat.Application.Services
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly ILogger<ChatRoomService> _logger;
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IMapper _mapper;
        private readonly ICustomMemoryCache _cache;
        private readonly IApplicationUserService _applicationUser;
        private readonly ICurrentUserService _currentUser;
        public ChatRoomService(IApplicationUserService applicationUser,ICurrentUserService currentUser,ICustomMemoryCache cache, IMapper mapper, ILogger<ChatRoomService> logger, IChatRoomRepository chatRoomRepository)
        {
            _applicationUser = applicationUser;
            _currentUser = currentUser;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _chatRoomRepository = chatRoomRepository;
        }
        public async Task<bool> IsUserInChatRoom(Guid chatRoomId, Guid userId)
        {
            var chatRoom = await _chatRoomRepository.GetChatRoomWithUsersByIdAsync(chatRoomId);
            if (chatRoom == null) return false;

            return chatRoom.ChatRoomUsers!.Any(u => u.UserId == userId);
        }
        public async Task<Result<bool>> ChatRoomExistsAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Please provide an Id");
                return Result<bool>.FailureResponse("Please provide an Id");
            }

            var result = await _chatRoomRepository.ChatRoomExistsAsync(chatRoomId);
            if (!result)
            {
                _logger.LogError("Chatroom does not exist");
                return Result<bool>.FailureResponse("Chatroom does not exist");
            }

            return Result<bool>.SuccessResponse(true,"ChatRoom exists");
        }
        public async Task<Result<ChatRoomDto?>> CreateChatRoomAsync(CreateChatRoomDto createChatRoomDto)
        {
            if (createChatRoomDto.IsGroup  && string.IsNullOrWhiteSpace(createChatRoomDto.Name) )
            {
                _logger.LogError("Please provide a name");
                return Result<ChatRoomDto?>.FailureResponse("Please provide a name");
            }

            if (createChatRoomDto.MemberIds == null || !createChatRoomDto.MemberIds.Any())
            {
                _logger.LogError("Please provide at least one member ID");
                return Result<ChatRoomDto?>.FailureResponse("Please provide at least one member ID");
            }

            _logger.LogInformation($"MemberIds count received: {createChatRoomDto.MemberIds?.Count}");

            //if (createChatRoomDto.IsGroup && createChatRoomDto!.MemberIds!.Count <= 2)
            //{
            //    _logger.LogError("Group chats must have more than two members.");
            //    return Result<ChatRoomDto?>.FailureResponse("Group chats must have more than two members.");
            //}

            if (!createChatRoomDto.IsGroup && createChatRoomDto.MemberIds!.Count != 2)
            {
                _logger.LogInformation("Private chat must have exactly two members.");
                return Result<ChatRoomDto?>.FailureResponse("Private chat must have exactly two members.");
            }

            if (!createChatRoomDto.IsGroup)
            {
                var existingPrivateChat = await GetPrivateChatAsync(createChatRoomDto!.MemberIds!);
                if (existingPrivateChat.Success && existingPrivateChat.Data != null)
                {
                    _logger.LogInformation("Private chat already exists between these members.");
                    return Result<ChatRoomDto?>.SuccessResponse(existingPrivateChat.Data);
                }
            }

            var crDtoToEntity = _mapper.Map<ChatRoom>(createChatRoomDto);

            foreach (var userId in createChatRoomDto.MemberIds!)
            {
                crDtoToEntity.ChatRoomUsers!.Add(new ChatRoomUser
                {
                    ChatRoomId = crDtoToEntity.ChatRoomId,
                    UserId = userId
                });
            }

            var createdChatRoom = await _chatRoomRepository.CreateChatRoomAsync(crDtoToEntity);

            var entityToDto = _mapper.Map<ChatRoomDto>(createdChatRoom);

            var currentUserId =  _currentUser.GetCurrentUserId();

            entityToDto.Users = createdChatRoom!.ChatRoomUsers!
                .Select(cru => cru.User)
                .Where(u => u.Id != currentUserId)
                .Select(u => new ApplicationUserDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    DpUrl = u.DpUrl!
                })
                .ToList();

            _cache.RemoveByPrefix($"chatRoomByName_");

            foreach (var userId in createChatRoomDto.MemberIds!)
            {
                _cache.Remove($"chatRoomRelatedToUser_{userId}");
            }

            return Result<ChatRoomDto?>.SuccessResponse(entityToDto,"ChatRoom created successfully");
        }
        public async Task<Result<ChatRoomDto?>> GetPrivateChatAsync(List<Guid> memberIds)
        {
            if (memberIds == null || memberIds.Count != 2)
            {
                _logger.LogError("Private chats must have exactly two members.");
                return Result<ChatRoomDto?>.FailureResponse("Private chats must have exactly two members.");
            }

            var existingPrivateChat = await _chatRoomRepository.GetPrivateChatRoomAsync(memberIds);

            if (existingPrivateChat != null)
            {
                var existingChatDto = _mapper.Map<ChatRoomDto>(existingPrivateChat);
                return Result<ChatRoomDto?>.SuccessResponse(existingChatDto, "Private chatRoom fetched successfully");
            }
            _logger.LogInformation("No private chat found. Creating a new one...");

            var newChatRoom = new CreateChatRoomDto
            {
                Name = "",
                IsGroup = false,
                MemberIds = memberIds
            };

            var crDtoToEntity = _mapper.Map<ChatRoom>(newChatRoom);
            foreach (var userId in memberIds!)
            {
                crDtoToEntity.ChatRoomUsers!.Add(new ChatRoomUser
                {
                    ChatRoomId = crDtoToEntity.ChatRoomId,
                    UserId = userId
                });
            }

            var createdChatRoom = await _chatRoomRepository.CreateChatRoomAsync(crDtoToEntity);

            var entityToDto = _mapper.Map<ChatRoomDto>(createdChatRoom);
            var currentUserId = _currentUser.GetCurrentUserId();

            entityToDto.Users = createdChatRoom!.ChatRoomUsers!
                          .Select(cru => cru.User)
                          .Where(u => u.Id != currentUserId)
                          .Select(u => new ApplicationUserDto
                          {
                              Id = u.Id,
                              Username = u.UserName!,
                              DpUrl = u.DpUrl!
                          })
                          .ToList();

            _cache.RemoveByPrefix($"chatRoomByName_");

            foreach (var userId in memberIds!)
            {
                _cache.Remove($"chatRoomRelatedToUser_{userId}");
            }
            return Result<ChatRoomDto?>.SuccessResponse(entityToDto, "ChatRoom created successfully");

        }
        public async Task<Result<List<ChatRoomDto>>> GetChatRoomsUserIsNotInAsync(Guid currentUserId)
        {
            if (currentUserId == Guid.Empty)
            {
                _logger.LogError("Please provide a valid user Id");
                return Result<List<ChatRoomDto>>.FailureResponse("Please provide a valid user Id");
            }

            try
            {
                var chatRooms = await _chatRoomRepository.GetChatRoomsthatUserIsNotIn(currentUserId);

                if (chatRooms == null || !chatRooms.Any())
                {
                    _logger.LogInformation("No chat rooms found that the user is not part of");
                    return Result<List<ChatRoomDto>>.SuccessResponse(new List<ChatRoomDto>(), "No chat rooms found that the user is not part of");
                }

                var currentUserIdLocal = _currentUser.GetCurrentUserId();

                var chatRoomDtos = chatRooms.Select(cr =>
                {
                    var dto = _mapper.Map<ChatRoomDto>(cr);
                    dto.Users = cr.ChatRoomUsers!
                        .Select(cru => cru.User!)
                        .Where(u => u.Id != currentUserIdLocal)
                        .Select(u => new ApplicationUserDto
                        {
                            Id = u.Id,
                            Username = u.UserName!,
                            DpUrl = u.DpUrl!
                        })
                        .ToList();
                    return dto;
                }).ToList();

                return Result<List<ChatRoomDto>>.SuccessResponse(chatRoomDtos, "Chat rooms fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chat rooms that user is not in");
                return Result<List<ChatRoomDto>>.FailureResponse("Failed to fetch chat rooms due to an error.");
            }
        }
        public async Task<Result<bool>> DeleteChatRoomAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Please provide a chatroom Id");
                return Result<bool>.FailureResponse("Please provide a chatroom Id");
            }

            // 1. Get all user IDs in the chat room BEFORE deleting
            var chatRoom = await _chatRoomRepository.GetChatRoomWithUsersByIdAsync(chatRoomId);
            var userIds = chatRoom?.ChatRoomUsers?.Select(cru => cru.UserId).ToList() ?? new List<Guid>();

            // 2. Delete the chat room (this will remove the room and all ChatRoomUser links)
            var result = await _chatRoomRepository.DeleteChatRoom(chatRoomId);
            if (!result)
            {
                _logger.LogError("Chatroom not deleted successfully");
                return Result<bool>.FailureResponse("Chatroom not deleted successfully");
            }

            // 3. Clear the cache for all affected users
            foreach (var userId in userIds)
            {
                _cache.Remove($"chatRoomRelatedToUser_{userId}");
            }

            return Result<bool>.SuccessResponse(true, "Chatroom deleted successfully.");
        }
        public async Task<Result<ChatRoomDto?>> GetChatRoomByIdAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Please provide an Id");
                return Result<ChatRoomDto?>.FailureResponse("Please provide an Id");
            }

            string cacheKey = $"chatRoomById_{chatRoomId}";

            if (!_cache.TryGetValue(cacheKey, out ChatRoomDto? cachedChatRoom))
            {
                _logger.LogInformation("Cache not found, hitting the database...");

                var chatRoom = await _chatRoomRepository.GetChatRoomByIdAsync(chatRoomId);
                if (chatRoom == null)
                {
                    _logger.LogError($"Chatroom with Id: {chatRoomId} does not exist");
                    return Result<ChatRoomDto?>.FailureResponse("Chatroom does not exist");
                }

                var chatRoomDto = _mapper.Map<ChatRoomDto>(chatRoom);
                var currentUserId = _currentUser.GetCurrentUserId();

                chatRoomDto.Users = chatRoom.ChatRoomUsers!
                    .Select(cru => cru.User!)
                    .Where(u => u.Id != currentUserId)
                    .Select(u => new ApplicationUserDto
                    {
                        Id = u.Id,
                        Username = u.UserName!,
                        DpUrl = u.DpUrl!
                    })
                    .ToList();

                cachedChatRoom = chatRoomDto;

                _cache.Set(cacheKey, cachedChatRoom, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

                return Result<ChatRoomDto?>.SuccessResponse(cachedChatRoom);
            }

            _logger.LogInformation("Cache hit, returning cached chat room...");
            return Result<ChatRoomDto?>.SuccessResponse(cachedChatRoom, "Chat Room fetched successfully.");
        }
        public async Task<Result<ChatRoomDto?>> GetChatRoomByNameAsync(string chatRoomName)
        {
            if (string.IsNullOrWhiteSpace(chatRoomName))
            {
                _logger.LogError("Please provide a name");
                return Result<ChatRoomDto?>.FailureResponse("Please provide a name");
            }

            string cacheKey = $"chatRoomByName_{chatRoomName}";

            if (!_cache.TryGetValue(cacheKey, out ChatRoomDto? cachedChatRoom))
            {
                _logger.LogInformation("Cache not found, hitting the database...");
                var chatRoom = await _chatRoomRepository.GetChatRoomByNameAsync(chatRoomName);
                if (chatRoom == null)
                {
                    _logger.LogError($"Chatroom with name: {chatRoomName} does not exist");
                    return Result<ChatRoomDto?>.FailureResponse("Chatroom does not exist");
                }

                var chatRoomDto = _mapper.Map<ChatRoomDto>(chatRoom);

                var currentUserId = _currentUser.GetCurrentUserId();

                chatRoomDto.Users = chatRoom.ChatRoomUsers!
                    .Select(cru => cru.User!)
                    .Where(u => u.Id != currentUserId)
                    .Select(u => new ApplicationUserDto
                    {
                        Id = u.Id,
                        Username = u.UserName!,
                        DpUrl = u.DpUrl!
                    })
                    .ToList();

                cachedChatRoom = chatRoomDto;

                _cache.Set(cacheKey, cachedChatRoom, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
            else
            {
                _logger.LogInformation("Cache hit, returning cached chat room...");
            }

            return Result<ChatRoomDto?>.SuccessResponse(cachedChatRoom, "ChatRoom fetched successfully.");
        }
        public async Task<Result<List<ChatRoomDto>>> GetChatRoomRelatedToUserAsync(Guid UserId)
        {
            if (UserId == Guid.Empty)
            {
                _logger.LogError("Please provide a UserId");
                return Result<List<ChatRoomDto>>.FailureResponse("Please provide a UserId");
            }

            var user = await _applicationUser.GetUserByIdAsync(UserId);

            if (user == null)
            {
                _logger.LogError($"User with provided id:{UserId} does not exist");
                return Result<List<ChatRoomDto>>.FailureResponse("User with provided Id was not found");
            }

            string cacheKey = $"chatRoomRelatedToUser_{UserId}";

            if (!_cache.TryGetValue(cacheKey, out List<ChatRoomDto>? cachedChatRoomList))
            {
                var chatRoomList = await _chatRoomRepository.GetChatRoomRelatedToUserAsync(UserId);
                if (chatRoomList == null)
                {
                    _logger.LogError("User is not in any chat room");
                    return Result<List<ChatRoomDto>>.SuccessResponse(new List<ChatRoomDto>());
                }

                foreach (var chatRoom in chatRoomList)
                {
                    _logger.LogInformation($"ChatRoom: {chatRoom.Name} has {chatRoom.ChatRoomUsers?.Count} users.");
                    foreach (var chatRoomUser in chatRoom.ChatRoomUsers!)
                    {
                        _logger.LogInformation($"User in ChatRoom: {chatRoomUser.User?.UserName}");
                    }
                }

                var chatRoomListDto = _mapper.Map<List<ChatRoomDto>>(chatRoomList);

                foreach (var chatRoomDto in chatRoomListDto)
                {
                    var correspondingChatRoom = chatRoomList.First(cr => cr.ChatRoomId == chatRoomDto.ChatRoomId);

                    chatRoomDto.Users = correspondingChatRoom.ChatRoomUsers!
                        .Select(cru => new ApplicationUserDto
                        {
                            Id = cru.User!.Id,
                            Username = cru.User.UserName!,
                            DpUrl = cru.User.DpUrl!
                        })
                        .ToList();

                    _logger.LogInformation($"ChatRoom {chatRoomDto.Name} has {chatRoomDto.Users.Count} users.");
                }

                // Sort chat rooms by LastMessageTimestamp descending (latest first)
                chatRoomListDto = chatRoomListDto
                    .OrderByDescending(cr => cr.LastMessageTimestamp ?? DateTime.MinValue)
                    .ToList();

                _logger.LogWarning("This is the chatRoomList: {ChatRoomList}", JsonSerializer.Serialize(chatRoomListDto, new JsonSerializerOptions { WriteIndented = true }));

                cachedChatRoomList = chatRoomListDto;

                _cache.Set(cacheKey, cachedChatRoomList, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                });
            }

            return Result<List<ChatRoomDto>>.SuccessResponse(cachedChatRoomList!, "Chat rooms related to User");
        }
        public async Task<Result<bool>> UpdateChatRoomNameAsync(Guid chatRoomId, string newName)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Please provide an Id");
                return Result<bool>.FailureResponse("Please provide an Id");
            }

            var chatRoom = await _chatRoomRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                _logger.LogError("ChatRoom with provided Id does not exist");
                return Result<bool>.FailureResponse("ChatRoom does not exist");
            }

            var result = await _chatRoomRepository.UpdateChatRoomName(chatRoomId, newName);
            if (!result)
            {
                _logger.LogError("ChatRoom name failed to update");
                return Result<bool>.FailureResponse("ChatRoom name failed to update");
            }

            _cache.RemoveByPrefix($"chatRoomByName_");
            _cache.Remove($"chatRoomRelatedToUser_");

            return Result<bool>.SuccessResponse(true,"ChatRoom name updated successfully.");
        }
        public async Task<Result<List<Guid>>> GetChatRoomIdsForUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Please provide a UserId");
                return Result<List<Guid>>.FailureResponse("Please provide a UserId");
            }

            var user = await _applicationUser.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError($"User with provided id: {userId} does not exist");
                return Result<List<Guid>>.FailureResponse("User with provided Id was not found");
            }

            string cacheKey = $"chatRoomIdsRelatedToUser_{userId}";

            if (!_cache.TryGetValue(cacheKey, out List<Guid>? cachedChatRoomIds))
            {
                _logger.LogInformation("Cache miss for chat room IDs, fetching from repository...");

                var chatRooms = await _chatRoomRepository.GetChatRoomRelatedToUserAsync(userId);
                if (chatRooms == null || !chatRooms.Any())
                {
                    _logger.LogInformation("User is not part of any chat rooms.");
                    cachedChatRoomIds = new List<Guid>();
                    return Result<List<Guid>>.FailureResponse("ChatRoom list is empty");
                }
                else
                {
                    cachedChatRoomIds = chatRooms.Select(cr => cr.ChatRoomId).ToList();
                }

                _cache.Set(cacheKey, cachedChatRoomIds, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }
            else
            {
                _logger.LogInformation("Cache hit for chat room IDs, returning cached data.");
            }

            return Result<List<Guid>>.SuccessResponse(cachedChatRoomIds!, "Chat room IDs fetched successfully.");
        }
        public async Task<Result<int>> GetUnreadMessageCountAsync(Guid chatRoomId, Guid userId)
        {
            if (chatRoomId == Guid.Empty)
            {
                return Result<int>.FailureResponse("Please, provide a chat room id");
            }
            if (userId == Guid.Empty)
            {
                return Result<int>.FailureResponse("Please, provide a user id");
            }

            var chatRoom = _chatRoomRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                return Result<int>.FailureResponse("Chat room with provided id does not exist");
            }
            var user = _applicationUser.GetUserByIdAsync(userId);
            if(user == null)
            {
                return Result<int>.FailureResponse("No user with provided id exists in our database");
            }
            var unreadMessagesCount = await _chatRoomRepository.GetUnreadMessageCountAsync(chatRoomId, userId);
            if (unreadMessagesCount < 0)
            {
                return Result<int>.FailureResponse("Error occurred while counting unread messages");
            }

            return Result<int>.SuccessResponse(unreadMessagesCount);
        }
        public async Task<Result<bool>> MarkMessagesAsReadAsync(Guid chatRoomId, Guid userId)
        {
            if (chatRoomId == Guid.Empty)
            {
                return Result<bool>.FailureResponse("Please, provide a chat room id");
            }

            if (userId == Guid.Empty)
            {
                return Result<bool>.FailureResponse("Please, provide a user id");
            }

            var chatRoom = await _chatRoomRepository.GetChatRoomByIdAsync(chatRoomId);
            if (chatRoom == null)
            {
                return Result<bool>.FailureResponse("Chat room with provided id does not exist");
            }

            var user = await _applicationUser.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.FailureResponse("No user with provided id exists in our database");
            }

            try
            {
                await _chatRoomRepository.MarkMessagesAsReadAsync(chatRoomId, userId);
                return Result<bool>.SuccessResponse(true, "Messages marked as read successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.FailureResponse($"Error occurred while marking messages as read: {ex.Message}");
            }
        }
    }
}
