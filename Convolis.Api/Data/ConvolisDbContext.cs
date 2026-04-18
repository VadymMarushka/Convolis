using Convolis.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convolis.Api.Data
{
    public class ConvolisDbContext : DbContext
    {
        public static Guid GlobalChatId = new Guid("acb72d86-bb66-4b29-bbbc-c88f464fed23");
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public ConvolisDbContext(DbContextOptions<ConvolisDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Username - унікальний, ми будем використовувати його для пошуку людей як у тг
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure Many-to-Many: User <-> Conversation
            modelBuilder.Entity<Participant>()
                .HasKey(p => new { p.UserId, p.ConversationId });

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany(u => u.Participants)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(p => p.ConversationId);

            // Configure Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            modelBuilder.Entity<Conversation>().HasData(new Conversation
            {
                Id = GlobalChatId,
                Name = "Global Chat 🌍"
            });
        }
    }
}
