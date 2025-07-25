
namespace SpagChat.Application.DTO.Messages
{
    public class MarkAsReadRequest
    {
        public required List<Guid> messageIds { get; set; }
        public required Guid UserId { get; set; }
    }
}
