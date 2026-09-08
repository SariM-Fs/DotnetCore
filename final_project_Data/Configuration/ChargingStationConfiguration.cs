using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class ChargingStationConfiguration : IEntityTypeConfiguration<ChargingStation>
    {
        public void Configure(EntityTypeBuilder<ChargingStation> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Location).IsRequired().HasMaxLength(200);
            builder.Property(s => s.ConnectorType).IsRequired().HasMaxLength(50);

            builder.HasMany(s => s.Spots)
                   .WithOne(sp => sp.Station)
                   .HasForeignKey(sp => sp.StationId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many: a station can offer several amenities, and the same
            // amenity (e.g. "WiFi") is shared across many stations. EF Core 8 maps
            // this through an implicit join table (StationAmenity) via skip navigations.
            builder.HasMany(s => s.Amenities)
                   .WithMany(a => a.Stations)
                   .UsingEntity(j => j.ToTable("StationAmenities"));
        }
    }
}
