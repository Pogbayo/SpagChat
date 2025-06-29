

namespace SpagChat.Application.DTO.Auth
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = default!;
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}

