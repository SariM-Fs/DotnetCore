using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Content).IsRequired().HasMaxLength(1000);
            builder.Property(r => r.DriverName).IsRequired().HasMaxLength(100);
            builder.Property(r => r.DecidedByName).HasMaxLength(100);
            builder.Property(r => r.Rating).IsRequired();

            builder.HasIndex(r => new { r.StationId, r.Status });
            builder.HasIndex(r => r.CreatedAt);

            builder.HasOne(r => r.Station)
                .WithMany()
                .HasForeignKey(r => r.StationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Driver)
                .WithMany()
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
        }
    }
}
