
namespace SpagChat.Application.DTO.ChatRoomUsers
{
    public class AddUserToChatRoomDto
    {
        public required string ChatRoomId { get; set; }
        public required string UserId { get; set; }
    }

}
