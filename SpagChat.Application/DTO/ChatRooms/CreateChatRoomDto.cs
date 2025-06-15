
namespace SpagChat.Application.DTO.ChatRooms
{
    public class CreateChatRoomDto
    {
        public required string Name { get; set; }        
        public bool IsGroup { get; set; }
        public List<Guid>? MemberIds { get; set; } = new List<Guid>();
    }
}
