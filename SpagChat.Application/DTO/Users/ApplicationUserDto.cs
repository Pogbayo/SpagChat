
namespace SpagChat.Application.DTO.Users
{
    public class ApplicationUserDto
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        public required string DpUrl { get; set; }
    }
}
