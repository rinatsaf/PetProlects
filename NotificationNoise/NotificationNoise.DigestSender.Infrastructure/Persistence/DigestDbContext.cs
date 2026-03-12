using Microsoft.EntityFrameworkCore;
using NotificationNoise.DigestSender.Domain;

namespace NotificationNoise.DigestSender.Infrastructure.Persistence;

public sealed class DigestDbContext : DbContext
{
    public DigestDbContext(DbContextOptions<DigestDbContext> options) : base(options)
    {
    }

    public DbSet<Digest> Digests => Set<Digest>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Digest>(e =>
        {
            e.ToTable("digests");
            e.HasKey(x => x.Id);

            e.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Period).HasMaxLength(16).IsRequired();
            e.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
        });
    }
}
