using AutoMapper;
using AutoMapper.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Interfaces.ICache;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Result;
using SpagChat.Domain.Entities;

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

        //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMxNDcxMWE0LTAzYzMtNGM4Mi1jZWNhLTA4ZGRhYWM3OWNhZCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkZWJheW9vbHV3YXNlZ3VuMzM1QGdtYWlsLmNvbSIsImV4cCI6MTc0OTk4ODM0MywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NTAwMSIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjUwMDEifQ.uRjNU0XYrAS_02W-slHlSsk-_qYWmETCOAAZDf5prCQ

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

            if (createChatRoomDto.IsGroup && createChatRoomDto!.MemberIds!.Count <= 2)
            {
                _logger.LogError("Group chats must have more than two members.");
                return Result<ChatRoomDto?>.FailureResponse("Group chats must have more than two members.");
            }

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

            if (existingPrivateChat == null)
            {
                _logger.LogInformation("No private chat found with these exact members.");
                return Result<ChatRoomDto?>.FailureResponse("No private chat found with these exact members.");
            }

            var chatRoomDto = _mapper.Map<ChatRoomDto>(existingPrivateChat);
            return Result<ChatRoomDto?>.SuccessResponse(chatRoomDto,"Private chatRoom fetched successfully");
        }


        public async Task<Result<bool>> DeleteChatRoomAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Please provide an Id");
                return Result<bool>.FailureResponse("Please provide an Id");
            }

            var result = await _chatRoomRepository.DeleteChatRoom(chatRoomId);
            if (!result)
            {
                _logger.LogError("ChatRoom not deleted successfully");
                return Result<bool>.FailureResponse("ChatRoom not deleted successfully");
            }

            return Result<bool>.SuccessResponse(true,"Chat Room deledted successfully.");
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
            return Result<ChatRoomDto?>.SuccessResponse(cachedChatRoom,"Chat Room fetched successfully.");
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

                if (chatRoomDto == null)
                {
                    _logger.LogError("Mapped chatRoomDto is null");
                    return Result<ChatRoomDto?>.FailureResponse("An unexpected error occurred: ChatRoomDto is null");
                }

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

            return Result<ChatRoomDto?>.SuccessResponse(cachedChatRoom,"ChatRoom fetched successfully.");
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

                if (chatRoomListDto == null)
                {
                    _logger.LogError("Chat room list is null");
                    return Result<List<ChatRoomDto>>.FailureResponse("An unexpected error occurred: Cached chat room list is null");
                }

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


                cachedChatRoomList = chatRoomListDto;

                _cache.Set(cacheKey, cachedChatRoomList, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }

            return Result<List<ChatRoomDto>>.SuccessResponse(cachedChatRoomList!,"Chat rooms related to User");
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
    }
}
