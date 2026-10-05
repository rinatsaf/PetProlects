using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MovieReviewConfiguration : IEntityTypeConfiguration<MovieReview>
{
    public void Configure(EntityTypeBuilder<MovieReview> builder)
    {
        builder.ToTable("MovieReviews");
        
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.MovieId, r.UserId })
            .IsUnique();

        builder.HasOne(mr => mr.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(mr => mr.UserId);
        
        builder.HasOne(mr => mr.Movie)
            .WithMany(m => m.Reviews)
            .HasForeignKey(mr => mr.MovieId);
        
        builder.Property(mr => mr.Rating)
            .IsRequired();

        builder.Property(mr => mr.Comment)
            .HasMaxLength(2000);
    }
}