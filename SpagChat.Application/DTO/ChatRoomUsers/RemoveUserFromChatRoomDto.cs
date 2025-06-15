namespace SpagChat.Application.DTO.ChatRoomUsers
{
    public class RemoveUserFromChatRoomDto
    {
        public Guid ChatRoomId { get; set; }
        public Guid UserId { get; set; }
    }
}
