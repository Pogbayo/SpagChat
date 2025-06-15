using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatRoomController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;

        public ChatRoomController(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomDto createChatRoomDto)
        {
            var result = await _chatRoomService.CreateChatRoomAsync(createChatRoomDto);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetChatRoomById), new { chatRoomId = result.Data!.ChatRoomId }, result);
        }


        [HttpDelete("{chatRoomId}")]
        [Authorize]
        public async Task<IActionResult> DeleteChatRoom(Guid chatRoomId)
        {
            var result = await _chatRoomService.DeleteChatRoomAsync(chatRoomId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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
    }
}
