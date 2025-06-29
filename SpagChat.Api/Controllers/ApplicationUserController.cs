using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpagChat.API.SignalR;
using SpagChat.Application.DTO.Auth;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationUserController : ControllerBase
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IHubContext<ChatHub> _hub;

        public ApplicationUserController(IApplicationUserService applicationUserService, IHubContext<ChatHub> hub)
        {
            _applicationUserService = applicationUserService;
            _hub = hub;
        }

        [HttpGet("all-users/{numberOfUsers}")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers(int numberOfUsers)
        {
            var result = await _applicationUserService.GetAllUsersAsync(numberOfUsers);
            return Ok(result);
        }

        [HttpGet("by-Id/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserById( Guid userId)
        {
            var result = await _applicationUserService.GetUserByIdAsync(userId);

            if (result == null || result.Data == null)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDetails)
        {
            var result = await _applicationUserService.LoginAsync(loginDetails);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto userDetails)
        {
            var result = await _applicationUserService.RegisterUserAsync(userDetails);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetUserById), new { userId = result.Data }, result);
        }

        [HttpDelete("delete-users")]
        public async Task<IActionResult> DeleteUsers([FromBody] List<Guid> userIds, [FromQuery] bool useParallel = false)
        {
            var result = await _applicationUserService.DeleteUsersAsync(userIds, useParallel);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
