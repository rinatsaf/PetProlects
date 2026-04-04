using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SeatType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.BasePrice)
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber })
            .IsUnique();

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Seats)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}