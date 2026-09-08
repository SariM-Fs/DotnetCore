using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class ChargingSessionConfiguration : IEntityTypeConfiguration<ChargingSession>
    {
        public void Configure(EntityTypeBuilder<ChargingSession> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.StartTime).IsRequired();

            builder.HasOne(s => s.Spot)
                   .WithMany()
                   .HasForeignKey(s => s.SpotId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Driver)
                   .WithMany(d => d.Sessions)
                   .HasForeignKey(s => s.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
