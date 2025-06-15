

namespace SpagChat.Application.DTO.ChatRoomUsers
{
    public class AddUsersToChatRoomDto
    {
        public required string ChatRoomId { get; set; }
        public required List<string> UserIds { get; set; }
    }
}
