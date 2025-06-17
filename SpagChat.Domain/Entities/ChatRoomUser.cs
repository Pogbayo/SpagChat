using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpagChat.Domain.Entities
{
    public class ChatRoomUser
    {
        public Guid ChatRoomUserId { get; set; } = Guid.NewGuid();
        public required Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public required Guid ChatRoomId { get; set; }

        [ForeignKey(nameof(ChatRoomId))]
        public ChatRoom ChatRoom { get; set; } = default!;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
