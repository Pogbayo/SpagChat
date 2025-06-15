using Microsoft.AspNetCore.Identity;

namespace SpagChat.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DpUrl { get; set; }
        public ICollection<ChatRoomUser> ChatRoomUsers { get; set; } = default!;
        public ICollection<Message> Messages { get; set; } = default!;
    }
}
