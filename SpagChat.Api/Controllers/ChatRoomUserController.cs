using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpagChat.Application.DTO.ChatRoomUsers;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatRoomUserController : ControllerBase
    {
        private readonly IChatRoomUserService _chatRoomUserService;

        public ChatRoomUserController(IChatRoomUserService chatRoomUserService)
        {
            _chatRoomUserService = chatRoomUserService;
        }

        [HttpPost("{chatRoomId}/users")]
        [Authorize]
        public async Task<IActionResult> AddUsersToChatRoom(Guid chatRoomId, [FromBody] List<Guid> userIds)
        {
            var result = await _chatRoomUserService.AddUsersToChatRoomAsync(chatRoomId, userIds);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{chatRoomId}/users")]
        [Authorize]
        public async Task<IActionResult> GetUsersFromChatRoom([FromQuery] Guid chatRoomId)
        {
            var result = await _chatRoomUserService.GetUsersFromChatRoomAsync(chatRoomId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{chatRoomId}/users/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveUserFromChatRoom(Guid  chatRoomId, Guid userId)
        {
            var userDetails = new RemoveUserFromChatRoomDto
            {
                ChatRoomId = chatRoomId,
                UserId = userId
            };
            var result = await _chatRoomUserService.RemoveUserFromChatRoomAsync(userDetails);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
