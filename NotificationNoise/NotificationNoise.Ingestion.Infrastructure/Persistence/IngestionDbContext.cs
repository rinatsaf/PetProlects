using Microsoft.EntityFrameworkCore;
using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Infrastructure.Persistence;

public sealed class IngestionDbContext : DbContext
{
    public IngestionDbContext(DbContextOptions<IngestionDbContext> options) :  base(options)
    {}
    
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<GmailToken> GmailTokens => Set<GmailToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var n = modelBuilder.Entity<Notification>();
        
        n.ToTable("notifications");
        n.HasKey(x => x.Id);
        
        n.Property(x => x.UserId).IsRequired().HasMaxLength(128);
        n.Property(x => x.Provider).IsRequired().HasMaxLength(32);
        n.Property(x => x.ExternalMessageId).IsRequired().HasMaxLength(128);

        n.Property(x => x.FromEmail).IsRequired().HasMaxLength(256);
        n.Property(x => x.FromName).HasMaxLength(256);
        n.Property(x => x.Subject).HasMaxLength(512);

        n.Property(x => x.ReceivedAt).IsRequired();
        n.Property(x => x.HasListUnsubscribe).IsRequired();
        
        n.HasIndex(x => new { x.UserId, x.Provider, x.ExternalMessageId }).IsUnique();

        var t = modelBuilder.Entity<GmailToken>();

        t.ToTable("gmail_tokens");
        t.HasKey(x => x.Id);

        t.Property(x => x.UserId).IsRequired().HasMaxLength(128);
        t.Property(x => x.Provider).IsRequired().HasMaxLength(32);
        t.Property(x => x.AccessTokenEncrypted).IsRequired();
        t.Property(x => x.RefreshTokenEncrypted);
        t.Property(x => x.ExpiresAt).IsRequired();
        t.Property(x => x.Scope).HasMaxLength(512).IsRequired();
        t.Property(x => x.TokenType).HasMaxLength(32).IsRequired();
        t.Property(x => x.UpdatedAt).IsRequired();

        t.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
