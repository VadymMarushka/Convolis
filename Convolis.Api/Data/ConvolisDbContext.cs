using Convolis.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Convolis.Api.Data
{
    /// <summary>
    /// Represents the database context for the Convolis application.
    /// Configures entity relationships, database constraints, and initial data seeding.
    /// </summary>
    public class ConvolisDbContext : DbContext
    {
        // Marked as readonly to prevent accidental reassignment during runtime
        public static readonly Guid GlobalChatId = new Guid("acb72d86-bb66-4b29-bbbc-c88f464fed23");
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public ConvolisDbContext(DbContextOptions<ConvolisDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce unique usernames, as they act as primary identifiers for user searches (similar to standard handles)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure Many-to-Many: User <-> Conversation via the Participant join entity
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

            // Configure One-to-Many relationships for Messages
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            // Seed the mandatory Global Chat into the database
            modelBuilder.Entity<Conversation>().HasData(new Conversation
            {
                Id = GlobalChatId,
                Name = "Global Chat 🌍"
            });
        }
    }
}
