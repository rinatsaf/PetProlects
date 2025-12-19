using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("city");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.BboxMaxLat)
            .IsRequired();
        builder.Property(x => x.BboxMaxLon)
            .IsRequired();
        builder.Property(x => x.BboxMinLat)
            .IsRequired();
        builder.Property(x => x.BboxMinLon)
            .IsRequired();
    }
}