using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpagChat.Domain.Entities;


namespace SpagChat.Infrastructure.Persistence
{
    public class SpagChatDb : IdentityDbContext<ApplicationUser,IdentityRole<Guid>,Guid>
    {
        public SpagChatDb(DbContextOptions<SpagChatDb> options)
           : base(options)
        {
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatRoomUser>()
                .HasKey(cru => cru.ChatUserRoomUserId);
        }
    }
}

