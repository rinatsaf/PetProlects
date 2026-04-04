using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
{
    public void Configure(EntityTypeBuilder<UserInteraction> builder)
    {
        builder.ToTable("user_interactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActionType)
            .IsRequired();

        builder.Property(x => x.WeightDelta)
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Interactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Movie)
            .WithMany(x => x.Interactions)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}