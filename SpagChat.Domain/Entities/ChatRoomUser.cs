using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SpagChat.Domain.Entities
{
    public class ChatRoomUser
    {
        public Guid ChatRoomUserId { get; set; } = Guid.NewGuid();
        public required Guid UserId { get; set; }

        [JsonIgnore]  
        public ApplicationUser User { get; set; } = default!;

        public required Guid ChatRoomId { get; set; }

        [ForeignKey(nameof(ChatRoomId))]
        [JsonIgnore]  
        public ChatRoom ChatRoom { get; set; } = default!;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}