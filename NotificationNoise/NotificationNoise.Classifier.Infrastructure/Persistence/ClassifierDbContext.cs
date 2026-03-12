using Microsoft.EntityFrameworkCore;
using NotificationNoise.Classifier.Domain;

namespace NotificationNoise.Classifier.Infrastructure.Persistence;

public sealed class ClassifierDbContext : DbContext
{
    public ClassifierDbContext(DbContextOptions<ClassifierDbContext> options) : base(options)
    {}
    
    public DbSet<Classification> Classifications => Set<Classification>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Classification>(e =>
        {
            e.ToTable("classifications");
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Provider).HasMaxLength(32).IsRequired();
            e.Property(x => x.ExternalMessageId).HasMaxLength(128).IsRequired();

            e.Property(x => x.Label).HasMaxLength(16).IsRequired();
            e.Property(x => x.ReasonsJson).HasColumnType("jsonb").IsRequired();

            e.HasIndex(x => new { x.UserId, x.Provider, x.ExternalMessageId }).IsUnique();
            e.HasIndex(x => x.NotificationId).IsUnique();
        });
    }
}