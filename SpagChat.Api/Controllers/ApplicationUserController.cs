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

        [HttpGet("all-users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers([FromQuery]int numberOfUsers)
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
    }
}
