using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(10, 2);

        builder.Property(x => x.TicketCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.TicketCode)
            .IsUnique();

        builder.HasIndex(x => new { x.SessionId, x.SeatId })
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Seat)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
