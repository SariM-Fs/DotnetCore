using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Email).IsRequired().HasMaxLength(150);
            builder.HasIndex(d => d.Email).IsUnique();
            builder.Property(d => d.LicensePlate).HasMaxLength(20);
            builder.Property(d => d.Role).IsRequired().HasMaxLength(20);
        }
    }
}
