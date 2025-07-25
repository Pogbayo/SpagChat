namespace SpagChat.Application.DTO.Messages
{
    public class MessageReadByDto
    {
        public required Guid UserId { get; set; }
        public required string Username { get; set; }
        public DateTime ReadAt { get; set; }
    }
}