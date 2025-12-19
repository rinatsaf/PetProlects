using Microsoft.EntityFrameworkCore;
using TouristRoutes.Models.Entity;
using Route = Microsoft.AspNetCore.Routing.Route;

namespace TouristRoutes.Data;

public class RoutesDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Poi>  Pois { get; set; }
    public DbSet<TourRoute> Routes { get; set; }
    
    public RoutesDbContext(DbContextOptions<RoutesDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoutesDbContext).Assembly);
    }
}