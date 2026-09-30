using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerExtractionAllowedHostConfiguration
    : IEntityTypeConfiguration<RetailerExtractionAllowedHost>
{
    public void Configure(EntityTypeBuilder<RetailerExtractionAllowedHost> builder)
    {
        builder.ToTable("RetailerExtractionAllowedHosts");
        builder.Property(host => host.NormalizedHost)
            .HasMaxLength(RetailerExtractionHost.MaxLength)
            .IsRequired();

        builder.HasIndex(host => new { host.ExtractionProfileId, host.NormalizedHost })
            .IsUnique();
        builder.HasOne(host => host.ExtractionProfile)
            .WithMany(profile => profile.AllowedHosts)
            .HasForeignKey(host => host.ExtractionProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
