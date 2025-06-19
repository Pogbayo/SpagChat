using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpagChat.API.SignalR;
using SpagChat.Application.DTO.Messages;
using SpagChat.Application.Interfaces.IServices;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageController(IMessageService messageService, IHubContext<ChatHub> hubContext)
    {
        _messageService = messageService;
        _hubContext = hubContext;
    }

    [HttpPost("send-message")]
    [Authorize]
    public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageDto messageDetails)
    {
        var result = await _messageService.SendMessageAsync(messageDetails);

        if (!result.Success)
            return BadRequest(result);

        await _hubContext.Clients.Group(messageDetails.ChatRoomId.ToString())
            .SendAsync("ReceiveMessage", result.Data);

        return Ok(result);
    }

    [HttpGet("chatroom/{chatRoomId}")]
    [Authorize]
    public async Task<IActionResult> GetMessagesByChatRoomId(Guid chatRoomId)
    {
        var result = await _messageService.GetMessagesByChatRoomIdAsync(chatRoomId);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("edit/{messageId}")]
    [Authorize]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] string newContent)
    {
        var result = await _messageService.EditMessageAsync(messageId, newContent);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("delete/{messageId}")]
    [Authorize]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var result = await _messageService.DeleteAsync(messageId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
