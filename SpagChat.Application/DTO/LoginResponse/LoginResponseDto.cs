using SpagChat.Application.DTO.Users;

namespace SpagChat.Application.DTO.LoginResponse
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public required ApplicationUserDto User { get; set; }
    }

}
