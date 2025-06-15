

namespace SpagChat.Application.DTO.Auth
{
    public class CreateUserDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}

