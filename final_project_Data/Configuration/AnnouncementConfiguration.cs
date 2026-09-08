using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Content).IsRequired().HasMaxLength(2000);
            builder.Property(a => a.CreatedByName).IsRequired().HasMaxLength(100);
            builder.HasIndex(a => a.CreatedAt);

            builder.HasOne(a => a.CreatedByDriver)
                .WithMany()
                .HasForeignKey(a => a.CreatedByDriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
