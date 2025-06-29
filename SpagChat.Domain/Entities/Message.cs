
using System.ComponentModel.DataAnnotations.Schema;


namespace SpagChat.Domain.Entities
{
    public class Message
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }


        [ForeignKey(nameof(ApplicationUser))]
        public required Guid SenderId { get; set; }
        public ApplicationUser Sender { get; set; } = default!;


        [ForeignKey(nameof(ChatRoom))]
        public  required Guid ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = default!;


    }
}
