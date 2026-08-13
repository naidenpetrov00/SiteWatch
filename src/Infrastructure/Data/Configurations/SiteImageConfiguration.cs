using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SiteImageConfiguration : IEntityTypeConfiguration<SiteImage>
{
    public void Configure(EntityTypeBuilder<SiteImage> builder)
    {
        builder.HasKey(si => new { si.SiteId, si.ImageId });

        builder.Property(si => si.Category)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(si => si.Site).WithMany(s => s.Images).HasForeignKey(si => si.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
