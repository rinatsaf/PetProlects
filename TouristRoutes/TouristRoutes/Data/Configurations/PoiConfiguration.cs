using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Data.Configurations;

public class PoiConfiguration : IEntityTypeConfiguration<Poi>
{
    public void Configure(EntityTypeBuilder<Poi> builder)
    {
        builder.ToTable("poi");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(250);

        builder.HasMany(x => x.Routes)
            .WithMany(r => r.Pois)
            .UsingEntity(j => j.ToTable("poi_route"));
        
        builder.HasOne(x => x.City)
            .WithMany(c => c.Pois)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}