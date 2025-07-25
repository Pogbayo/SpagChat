using System.Text.Json.Serialization; 

namespace SpagChat.Domain.Entities
{
    public class MessageReadBy
    {
        public Guid MessageId { get; set; }

        [JsonIgnore] 
        public Message? Message { get; set; }

        public Guid UserId { get; set; }

        [JsonIgnore]  
        public ApplicationUser? User { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}