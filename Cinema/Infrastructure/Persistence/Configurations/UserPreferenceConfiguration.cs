using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FeatureKey)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Weight)
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.UserId, x.FeatureKey })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Preferences)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}