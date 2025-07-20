
namespace SpagChat.Application.DTO.Messages
{
    public class MarkAsReadRequest
    {
        public required Guid ChatRoomId { get; set; }
        public required Guid UserId { get; set; }
    }
}
