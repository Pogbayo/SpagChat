using System.ComponentModel.DataAnnotations;

namespace SpagChat.Domain.Entities
{
    public class ChatRoom
    {
        public required Guid ChatRoomId { get; set; }
        public string? Name { get; set; }  
        public bool IsGroup { get; set; }

        public ICollection<ChatRoomUser>? ChatRoomUsers { get; set; } = new List<ChatRoomUser>();
        public ICollection<Message>? Messages { get; set; } = new List<Message>();

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
