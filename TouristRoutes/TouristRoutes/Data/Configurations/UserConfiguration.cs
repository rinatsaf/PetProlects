using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");
        
        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(25)
            .HasColumnType("varchar(255)");;
        
        builder.HasMany(x => x.Route)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Email).IsUnique();
        
        builder.Property(x => x.CreatedAt)
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );
    }
}