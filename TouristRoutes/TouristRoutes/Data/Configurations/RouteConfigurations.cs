using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<TourRoute>
{
    public void Configure(EntityTypeBuilder<TourRoute> builder)
    {
        builder.ToTable("routes");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}