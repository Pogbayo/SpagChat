using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; 

namespace SpagChat.Domain.Entities
{
    public class Message
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public required string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsEdited { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        [JsonIgnore]  
        public ICollection<MessageReadBy> ReadBy { get; set; } = new Collection<MessageReadBy>();

        [ForeignKey(nameof(ApplicationUser))]
        public required Guid SenderId { get; set; }

        [JsonIgnore]  
        public ApplicationUser Sender { get; set; } = default!;

        [ForeignKey(nameof(ChatRoom))]
        public required Guid ChatRoomId { get; set; }

        [JsonIgnore] 
        public ChatRoom ChatRoom { get; set; } = default!;
    }
}