using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Backend.Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class AvanciraDbContext : IdentityDbContext<User>
{
    public DbSet<StoredEvent> Events { get; set; }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<Country> Countries { get; set; }

    public DbSet<Referral> Referrals { get; set; }

    public DbSet<LessonCategory> LessonCategories { get; set; }
    public DbSet<ListingLessonCategory> ListingLessonCategories { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<UserCard> UserCards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletLog> WalletLogs { get; set; }

    public AvanciraDbContext(DbContextOptions<AvanciraDbContext> options)
        : base(options)
    {
        Events = Set<StoredEvent>();
        Addresses = Set<Address>();
        Countries = Set<Country>();
        Referrals = Set<Referral>();
        LessonCategories = Set<LessonCategory>();
        ListingLessonCategories = Set<ListingLessonCategory>();
        Lessons = Set<Lesson>();
        Listings = Set<Listing>();
        Reviews = Set<Review>();
        Chats = Set<Chat>();
        Messages = Set<Message>();
        Subscriptions = Set<Subscription>();
        SubscriptionHistories = Set<SubscriptionHistory>();
        PromoCodes = Set<PromoCode>();
        UserCards = Set<UserCard>();
        Transactions = Set<Transaction>();
        Wallets = Set<Wallet>();
        WalletLogs = Set<WalletLog>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Decimal precision settings
        modelBuilder.Entity<PromoCode>()
            .Property(p => p.DiscountAmount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Lesson>()
            .Property(l => l.Price)
            .HasPrecision(18, 4);


        modelBuilder.Entity<Subscription>()
            .Property(s => s.Amount)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Amount).HasPrecision(18, 4);
            entity.Property(t => t.PlatformFee).HasPrecision(18, 4);
            entity.Property(t => t.RefundAmount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<User>()
            .Property(u => u.TutorRefundRetention)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 4);

        modelBuilder.Entity<WalletLog>(entity =>
        {
            entity.Property(wl => wl.AmountChanged).HasPrecision(18, 4);
            entity.Property(wl => wl.NewBalance).HasPrecision(18, 4);
        });


        modelBuilder.Entity<ListingLessonCategory>()
        .HasKey(x => new { x.ListingId, x.LessonCategoryId });

            modelBuilder.Entity<ListingLessonCategory>()
                .HasOne(x => x.Listing)
                .WithMany(l => l.ListingLessonCategories)
                .HasForeignKey(x => x.ListingId);

            modelBuilder.Entity<ListingLessonCategory>()
                .HasOne(x => x.LessonCategory)
                .WithMany(c => c.ListingLessonCategories)
                .HasForeignKey(x => x.LessonCategoryId);

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


        // Configure Referrals entity
        modelBuilder.Entity<Referral>()
            .HasOne(r => r.Referrer)
            .WithMany()
            .HasForeignKey(r => r.ReferrerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Referral>()
            .HasOne(r => r.Referred)
            .WithMany()
            .HasForeignKey(r => r.ReferredId)
            .OnDelete(DeleteBehavior.Restrict);
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

    private string? GetUserId()
    {
        var httpContextAccessor = this.GetService<IHttpContextAccessor>();
        var userIdClaim = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            return null;
        }

        return userIdClaim.Value;
    }

    private void HandleAuditableEntities()
    {
        var userId = GetUserId();

        // Handle entities implementing IOwnable
        foreach (var entry in ChangeTracker.Entries<IOwnable>())
        {
            if (entry.State == EntityState.Added && !string.IsNullOrEmpty(userId))
            {
                entry.Entity.UserId = userId;
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

