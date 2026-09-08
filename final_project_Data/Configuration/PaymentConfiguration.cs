using final_project_Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace final_project_Data.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Method).IsRequired().HasMaxLength(20);
            builder.Property(p => p.CardLast4).HasMaxLength(4);
            builder.Property(p => p.NationalId).HasMaxLength(20);
            builder.Property(p => p.PhoneNumber).HasMaxLength(20);
            builder.Property(p => p.Status).IsRequired().HasMaxLength(20);

            builder.HasOne(p => p.Session)
                   .WithMany()
                   .HasForeignKey(p => p.SessionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
