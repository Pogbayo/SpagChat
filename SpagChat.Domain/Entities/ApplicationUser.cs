using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization; 

namespace SpagChat.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DpUrl { get; set; }

        [JsonIgnore] 
        public ICollection<ChatRoomUser> ChatRoomUsers { get; set; } = default!;

        [JsonIgnore]  
        public ICollection<Message> Messages { get; set; } = default!;
    }
}