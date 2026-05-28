using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SiteFileConfiguration : IEntityTypeConfiguration<SiteFile>
{
    public void Configure(EntityTypeBuilder<SiteFile> builder)
    {
        builder.HasKey(sf => new { sf.SiteId, sf.FileId });

        builder.Property(sf => sf.FileName)
            .HasMaxLength(512);

        builder.Property(sf => sf.ContentType)
            .HasMaxLength(128);

        builder.HasOne(sf => sf.Site)
            .WithMany(s => s.Files)
            .HasForeignKey(sf => sf.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
