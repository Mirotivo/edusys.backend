using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

public class skillseekDbContext : DbContext
{
    private readonly IConfiguration _config;

    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Referral> Referrals { get; set; }


    public DbSet<LessonCategory> LessonCategories { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<Subscription> Subscriptions { get; internal set; }
    public DbSet<UserCard> UserCards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletLog> WalletLogs { get; set; }

    public skillseekDbContext(DbContextOptions<skillseekDbContext> options, IConfiguration config)
        : base(options)
    {
        _config = config;
        Users = Set<User>();
        PasswordResetTokens = Set<PasswordResetToken>();
        Referrals = Set<Referral>();
        LessonCategories = Set<LessonCategory>();
        Lessons = Set<Lesson>();
        Listings = Set<Listing>();
        Reviews = Set<Review>();
        Chats = Set<Chat>();
        Messages = Set<Message>();
        Subscriptions = Set<Subscription>();
        UserCards = Set<UserCard>();
        Transactions = Set<Transaction>();
        Wallets = Set<Wallet>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Review configuration
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewee)
            .WithMany()
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Chat configuration
        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.Tutor)
            .WithMany()
            .HasForeignKey(c => c.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message configuration
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);


        // Transactions configuration
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Sender)
            .WithMany()
            .HasForeignKey(t => t.SenderId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Recipient)
            .WithMany()
            .HasForeignKey(t => t.RecipientId);

        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithOne()
            .HasForeignKey<Wallet>(w => w.UserId);

        // Configure relationships
        modelBuilder.Entity<UserCard>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCards)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete when a user is deleted

        // Configure ListingRates entity
        modelBuilder.Entity<ListingRates>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.Listing)
                .WithOne(l => l.Rates)
                .HasForeignKey<ListingRates>(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete for Rates
        
        // Configure Referrals entity
        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.ReferrerId)
                .OnDelete(DeleteBehavior.Restrict); // Ensure referrer is not deleted when referral exists

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.ReferredId)
                .OnDelete(DeleteBehavior.Restrict); // Ensure referred user is not deleted when referral exists
        });

        });
    }


    public override int SaveChanges()
    {
        return SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTokensForNewUsers();
        HandleAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTokensForNewUsers()
    {
        foreach (var entry in ChangeTracker.Entries<User>().Where(e => e.State == EntityState.Added))
        {
            var user = entry.Entity;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string token;
            do
            {
                token = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[new Random().Next(s.Length)]).ToArray());
            } while (Users.Any(u => u.RecommendationToken == token));
            user.RecommendationToken = token;
        }
    }

    private int? GetUserId()
    {
        var httpContextAccessor = this.GetService<IHttpContextAccessor>();
        var userIdClaim = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    private void HandleAuditableEntities()
    {
        var userId = GetUserId();

        // Handle entities implementing IOwnable
        foreach (var entry in ChangeTracker.Entries<IOwnable>())
        {
            if (entry.State == EntityState.Added && userId != null)
            {
                entry.Entity.UserId = userId.Value;
            }
        }


        // Handle entities implementing ICreatable
        foreach (var entry in ChangeTracker.Entries<ICreatable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        // Handle entities implementing IUpdatable
        foreach (var entry in ChangeTracker.Entries<IUpdatable>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Handle entities implementing IDeletable
        foreach (var entry in ChangeTracker.Entries<IDeletable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Active = true;
                entry.Entity.DeletedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (!entry.Entity.Active && entry.Entity.DeletedAt == null)
                {
                    // Set DeletedAt when deactivating
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                }
                else if (entry.Entity.Active && entry.Entity.DeletedAt != null)
                {
                    // Clear DeletedAt when reactivating
                    entry.Entity.DeletedAt = null;
                }
            }
        }
    }
}
