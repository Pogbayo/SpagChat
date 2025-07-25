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
        public DbSet<MessageReadBy> MessageReadBy { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatRoomUser>(entity =>
            {
                entity.HasKey(cru => cru.ChatRoomUserId);
                entity.Property(cr => cr.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.Property(cr => cr.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<MessageReadBy>()
             .HasKey(mrb => new { mrb.MessageId, mrb.UserId });

            modelBuilder.Entity<MessageReadBy>()
                .HasOne(mrb => mrb.Message)
                .WithMany(m => m.ReadBy)
                .HasForeignKey(mrb => mrb.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<MessageReadBy>()
            //    .HasOne(mrb => mrb.User)
            //    .WithMany(u => u.MessagesRead)
            //    .HasForeignKey(mrb => mrb.UserId);
        }
    }
}

