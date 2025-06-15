using SpagChat.Application.DTO.Users; 

namespace SpagChat.Application.DTO.ChatRooms
{
    public class ChatRoomDto
    {
        public required Guid ChatRoomId { get; set; }
        public required string Name { get; set; }
        public bool IsGroup { get; set; }
        public required string LastMessageContent { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
        public List<ApplicationUserDto> Users { get; set; } = new();
    }
}
