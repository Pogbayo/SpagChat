﻿using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.Messages;
using SpagChat.Application.Interfaces.ICache;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Result;
using SpagChat.Domain.Entities;
using System.Text.Json;

namespace SpagChat.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly ICustomMemoryCache _cache;
        private readonly IChatRoomService _chatRoomService;

        public MessageService(IChatRoomService chatRoomservice, ICustomMemoryCache cache, IMapper mapper, ILogger<MessageService> logger, IMessageRepository messageRepository)
        {
            _chatRoomService = chatRoomservice;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _messageRepository = messageRepository;
        }

        public async Task<Result<bool>> DeleteAsync(Guid messageId)
        {
            if (messageId == Guid.Empty)
            {
                _logger.LogError("Message with provided id does not exist");
                return Result<bool>.FailureResponse("Message with provided id does not exist");
            }
            
            var result = await _messageRepository.DeleteAsync(messageId);
            if (!result)
            {
                _logger.LogError("Message not deleted SuccessResponsefully");
                return Result<bool>.FailureResponse("Message not deleted SuccessResponsefully");
            }
            return Result<bool>.SuccessResponse(true,"Message deleted succssfully");
        }
        public async Task<Result<bool>> EditMessageAsync(Guid messageId, string newContent)
        {
            if (messageId == Guid.Empty)
            {
                _logger.LogError("Message with provided id does not exist");
                return Result<bool>.FailureResponse("Message with provided id does not exist");
            }

            if (string.IsNullOrWhiteSpace(newContent))
            {
                _logger.LogError("Please, provide a new content");
                return Result<bool>.FailureResponse("Please, provide a new content");
            }

            var result = await _messageRepository.EditMessageAsync(messageId, newContent);
            if (!result)
            {
                _logger.LogError("Message editing FailureResponseed");
                return Result<bool>.FailureResponse("Message editing FailureResponseed");
            }

            return Result<bool>.SuccessResponse(true, "Message edited successfully");
        }
        public async Task<Result<IEnumerable<MessageDto>>> GetMessagesByChatRoomIdAsync(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty)
            {
                _logger.LogError("Invalid ChatRoom Id provided");
                return Result<IEnumerable<MessageDto>>.FailureResponse("Invalid ChatRoom Id provided");
            }

            string cacheKey = $"ChatRoomMessages_{chatRoomId}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<MessageDto> cachedMessages))
            {
                _logger.LogInformation("Cache not found, retrieving from database...");

                var messagesByChatroom = await _messageRepository.GetMessagesByChatRoomIdAsync(chatRoomId);
                _logger.LogInformation("Raw messages from repository: {Count}", messagesByChatroom?.Count() ?? 0);

                var mappedMessages = _mapper.Map<IEnumerable<MessageDto>>(messagesByChatroom);

                if (mappedMessages == null)
                {
                    _logger.LogError("Mapping error");
                    return Result<IEnumerable<MessageDto>>.FailureResponse("Mapping error occurred");
                }

                _cache.Set(cacheKey, mappedMessages, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                });

                cachedMessages = mappedMessages;
            }
            else
            {
                _logger.LogInformation("Cache hit, returning cached messages...");
            }

            // Log the count of messages returned (whether from cache or DB)
            int messageCount = cachedMessages?.Count() ?? 0;
            _logger.LogInformation("Returning {Count} messages for chatRoom {ChatRoomId}", messageCount, chatRoomId);

            return Result<IEnumerable<MessageDto>>.SuccessResponse(cachedMessages!, "Messages in chatRoom fetched successfully.");
        }
        public async Task<Result<MessageDto>> SendMessageAsync(SendMessageDto messageDetails)
        {
            if (messageDetails.SenderId == Guid.Empty || messageDetails.ChatRoomId == Guid.Empty)
            {
                _logger.LogError("UserId or ChatRoomId is missing");
                return Result<MessageDto>.FailureResponse("UserId or ChatRoomId is missing");
            }

            if (string.IsNullOrWhiteSpace(messageDetails.Content))
            {
                _logger.LogError("Message content cannot be empty");
                return Result<MessageDto>.FailureResponse("Message content cannot be empty");
            }

            var chatRoomResult = await _chatRoomService.GetChatRoomByIdAsync(messageDetails.ChatRoomId);
            if (!chatRoomResult.Success || chatRoomResult.Data == null)
            {
                _logger.LogWarning("Chat Room with provided Id does not exist");
                return Result<MessageDto>.FailureResponse("ChatRoom does not exist", "Chat room with provided Id does not exist");
            }

            var isInRoom = await _chatRoomService.IsUserInChatRoom(messageDetails.ChatRoomId, messageDetails.SenderId);
            if (!isInRoom)
            {
                _logger.LogWarning("User cannot send message to a group they are not in...");
                return Result<MessageDto>.FailureResponse("Unauthorized");
            }

            var messageEntity = _mapper.Map<Message>(messageDetails);
            var savedMessage = await _messageRepository.SendMessageAsync(messageEntity);

            if (savedMessage == null)
            {
                return Result<MessageDto>.FailureResponse("Error sending message..");
            }
            await _messageRepository.AddMessageReadByAsync(savedMessage.MessageId, messageDetails.SenderId);

            var messageDto = _mapper.Map<MessageDto>(savedMessage);
            messageDto.ChatRoomId = messageDetails.ChatRoomId;

            _cache.Remove($"ChatRoomMessages_{messageDetails.ChatRoomId}");
            _cache.Remove($"chatRoomById_{messageDetails.ChatRoomId}");

            return Result<MessageDto>.SuccessResponse(messageDto, "Message sent successfully");
        }
    }
}
