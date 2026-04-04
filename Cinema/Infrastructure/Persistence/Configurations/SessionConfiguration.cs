using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BasePrice)
            .HasPrecision(10, 2);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => new { x.MovieId, x.StartTime });
        builder.HasIndex(x => new { x.HallId, x.StartTime });

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}