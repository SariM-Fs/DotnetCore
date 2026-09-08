using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;


namespace final_project_Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ChargingStation> ChargingStations => Set<ChargingStation>();
        public DbSet<ChargingSpot> ChargingSpots => Set<ChargingSpot>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Amenity> Amenities => Set<Amenity>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
