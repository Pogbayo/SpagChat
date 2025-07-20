
namespace SpagChat.Application.DTO.Auth
{
    public class UpdateUsernameDto
    {
        public Guid UserId { get; set; }
        public string NewUsername { get; set; } = string.Empty;
    }
}
