namespace SpagChat.Application.DTO.Messages
{
    public class SendMessageDto
    {
        public required Guid ChatRoomId { get; set; }
        public required Guid SenderId { get; set; }
        public required string Content { get; set; }
    }
}
