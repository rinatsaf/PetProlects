using Microsoft.EntityFrameworkCore;
using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Infrastructure.Persistence;

public sealed class InsightsDbContext : DbContext
{
    public InsightsDbContext(DbContextOptions<InsightsDbContext> options) : base(options) {}

    public DbSet<SenderStats> SenderStats => Set<SenderStats>();
    public DbSet<DailyStats> DailyStats => Set<DailyStats>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<SenderStats>(e =>
        {
            e.ToTable("sender_stats");
            e.HasKey(x => new { x.UserId, x.SenderKey });

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.SenderKey).HasMaxLength(256).IsRequired();
            e.Property(x => x.LastSeenAt).IsRequired();
        });

        mb.Entity<DailyStats>(e =>
        {
            e.ToTable("daily_stats");
            e.HasKey(x => new { x.UserId, x.Date });

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Date).HasColumnType("date").IsRequired();
        });

        mb.Entity<Recommendation>(e =>
        {
            e.ToTable("recommendations");
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Target).HasMaxLength(256).IsRequired();
            e.Property(x => x.Why).HasMaxLength(512).IsRequired();

            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

            e.HasIndex(x => new { x.UserId, x.Status });
        });
    }
}
