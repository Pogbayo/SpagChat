
namespace SpagChat.Application.DTO.Auth
{
    public class UpdatePasswordDto
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
