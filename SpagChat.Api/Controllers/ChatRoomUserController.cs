using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpagChat.API.SignalR;
using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Domain.Entities;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatRoomUserController : ControllerBase
    {
        private readonly IChatRoomUserService _chatRoomUserService;
        private readonly ILogger<ChatRoomController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRoomUserController(IHubContext<ChatHub> hubContext,IChatRoomUserService chatRoomUserService, ILogger<ChatRoomController> logger)
        {
            _hubContext = hubContext;
            _chatRoomUserService = chatRoomUserService;
            _logger = logger;

        }

        [HttpPost("{chatRoomId}/users")]
        [Authorize]
        public async Task<IActionResult> AddUsersToChatRoom(Guid chatRoomId, [FromBody] List<Guid> userIds)
        {
            _logger.LogInformation($"[{DateTime.UtcNow}] AddUsersToChatRoom endpoint called with ChatRoomId: {chatRoomId} and Users: {string.Join(", ", userIds)}");

            var result = await _chatRoomUserService.AddUsersToChatRoomAsync(chatRoomId, userIds);

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.Group(chatRoomId.ToString()).SendAsync("UsersChanged", new
            {
                Event = "UsersAdded",
                UserIds = userIds
            });

            return Ok(result);
        }


        [HttpGet("{chatRoomId}/users")]
        [Authorize]
        public async Task<IActionResult> GetUsersFromChatRoom( Guid chatRoomId)
        {
            var result = await _chatRoomUserService.GetUsersFromChatRoomAsync(chatRoomId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{chatRoomId}/users/")]
        [Authorize]
        public async Task<IActionResult> RemoveUserFromChatRoom(Guid  chatRoomId,[FromBody] Guid userId)
        {
            var userDetails = new RemoveUserFromChatRoomDto
            {
                ChatRoomId = chatRoomId,
                UserId = userId
            };
            var result = await _chatRoomUserService.RemoveUserFromChatRoomAsync(userDetails);

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.Group(chatRoomId.ToString()).SendAsync("UsersChanged", new
            {
                Event = "UserRemoved",
                UserId = userId
            });

            return Ok(result);
        }

        [HttpGet("non-mutual-friends/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetNonMutualFriends(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Empty user ID provided to GetNonMutualFriends.");
                return BadRequest("Invalid user ID.");
            }

            var result = await _chatRoomUserService.GetNonMutualFriendsAsync(userId);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to get non-mutual friends for user {UserId}. Error: {Error}", userId, result.Error);
                return NotFound(result.Message ?? "No non-mutual friends found.");
            }

            return Ok(result);
        }

    }
}
