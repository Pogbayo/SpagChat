using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpagChat.API.SignalR;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.DTO.Messages;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Services;
using SpagChat.Domain.Entities;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatRoomController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly ILogger<ChatRoomController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
  
        public ChatRoomController(ILogger<ChatRoomController> logger,IChatRoomService chatRoomService, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _chatRoomService = chatRoomService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto createChatRoomDto)
        {
            var result = await _chatRoomService.CreateChatRoomAsync(createChatRoomDto);

            if (!result.Success)
                return BadRequest(result);

            var memberIds = createChatRoomDto.MemberIds!.Select(id => id.ToString()).ToList();

            await _hubContext.Clients.Users(memberIds)
                .SendAsync("ChatRoomCreated", result.Data);

            return CreatedAtAction(nameof(GetChatRoomById), new { chatRoomId = result.Data!.ChatRoomId }, result);
        }

        [HttpDelete("{chatRoomId}")]
        [Authorize]
        public async Task<IActionResult> DeleteChatRoom(Guid chatRoomId)
        {
            var result = await _chatRoomService.DeleteChatRoomAsync(chatRoomId);

            if (!result.Success)
                return BadRequest(result);
            await _hubContext.Clients.All.SendAsync("ChatRoomDeleted", chatRoomId);

            return Ok(result);
        }

        [HttpGet("unread-count/{chatRoomId}")]
        [Authorize]
        public async Task<IActionResult> GetUnreadMessageCount(Guid chatRoomId, Guid userId)
        {
                if (chatRoomId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Invalid chat room ID" });
                }

                if (userId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Invalid user ID" });
                }

                var result = await _chatRoomService.GetUnreadMessageCountAsync(chatRoomId, userId);

                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
        }

        [HttpPost("mark-as-read")]
        [Authorize]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkAsReadRequest request)
        {
            if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

           var result = await _chatRoomService.MarkMessagesAsReadAsync(request.ChatRoomId, request.UserId);

             if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
         }
       
        [HttpGet("{chatRoomId}")]
        [Authorize]
        public async Task<IActionResult> GetChatRoomById(Guid chatRoomId)
        {
            var result = await _chatRoomService.GetChatRoomByIdAsync(chatRoomId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
 
        [HttpGet("by-name/{chatRoomName}")]
        [Authorize]
        public async Task<IActionResult> GetChatRoomByName(string chatRoomName)
        {
            var result = await _chatRoomService.GetChatRoomByNameAsync(chatRoomName);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetChatRoomsRelatedToUser(Guid userId)
        {
            var result = await _chatRoomService.GetChatRoomRelatedToUserAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
      
        [HttpPatch("{chatRoomId}/name")]
        [Authorize]
        public async Task<IActionResult> UpdateChatRoomName(Guid chatRoomId, [FromQuery] string newName)
        {
            var result = await _chatRoomService.UpdateChatRoomNameAsync(chatRoomId, newName);

            if (!result.Success)
                return BadRequest(result);
            await _hubContext.Clients.All.SendAsync("ChatRoomUpdated", chatRoomId, newName);

            return Ok(result);
        }

        [HttpGet("{chatRoomId}/exists")]
        [Authorize]
        public async Task<IActionResult> ChatRoomExists(Guid chatRoomId)
        {
            var result = await _chatRoomService.ChatRoomExistsAsync(chatRoomId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("get-private-chat")]
        [Authorize]
        public async Task<IActionResult> GetPrivateChatAsync([FromQuery] Guid currentUserId, [FromQuery] Guid friendUserId)
        {
             List<Guid> memberIds = new List<Guid>
            {
              currentUserId,
              friendUserId
            };

            var result = await _chatRoomService.GetPrivateChatAsync(memberIds);

            if (!result.Success)
                return NotFound(result);
            foreach (var user in result.Data!.Users)
            {
                if (ChatHub.OnlineUsers.TryGetValue(user.Id.ToString(), out var connectionId))
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ChatRoomCreated", result.Data);
                }
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("chatroomsids/{userId}")]
        public async Task<IActionResult> GetUserChatRoomsIds(Guid userId)
        {
            var result = await _chatRoomService.GetChatRoomIdsForUserAsync(userId);
            if (result.Success && result.Data != null)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("not-in/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetChatRoomsUserIsNotIn(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid user Id.");
            }

            var result = await _chatRoomService.GetChatRoomsUserIsNotInAsync(userId);

            if (!result.Success)
            {
                _logger.LogError("Failed to get chat rooms user is not in: {Error}", result.Error);
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
