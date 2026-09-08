using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class ChargingSpotConfiguration : IEntityTypeConfiguration<ChargingSpot>
    {
        public void Configure(EntityTypeBuilder<ChargingSpot> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.SpotNumber).IsRequired();
            builder.Property(s => s.Status).IsRequired();
            builder.Property(s => s.RowVersion).IsRowVersion();

            builder.HasIndex(s => new { s.StationId, s.SpotNumber }).IsUnique();
        }
    }
}
