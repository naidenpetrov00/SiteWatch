using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SiteVideoConfiguration : IEntityTypeConfiguration<SiteVideo>
{
    public void Configure(EntityTypeBuilder<SiteVideo> builder)
    {
        builder.HasKey(sv => new { sv.SiteId, sv.VideoId });

        builder.Property(sv => sv.DurationSeconds)
            .HasColumnType("int");

        builder.HasOne(sv => sv.Site)
            .WithMany(s => s.Videos)
            .HasForeignKey(sv => sv.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
