using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerExtractionProfileConfiguration
    : IEntityTypeConfiguration<RetailerExtractionProfile>
{
    public void Configure(EntityTypeBuilder<RetailerExtractionProfile> builder)
    {
        builder.ToTable(
            "RetailerExtractionProfiles",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RetailerExtractionProfiles_Lifecycle",
                    "(([Status] = N'Draft' AND [IsActive] = 0 AND [PublishedAt] IS NULL AND [PublishedBy] IS NULL) OR "
                    + "([Status] = N'Published' AND [PublishedAt] IS NOT NULL AND [PublishedBy] IS NOT NULL "
                    + "AND LEN(LTRIM(RTRIM([PublishedBy]))) > 0)) "
                    + "AND ([IsActive] = 0 OR [Status] = N'Published')");
                table.HasCheckConstraint(
                    "CK_RetailerExtractionProfiles_Version",
                    "[Version] > 0");
            });

        builder.Property(profile => profile.Version).IsRequired();
        builder.Property(profile => profile.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(profile => profile.IsActive).IsRequired();
        builder.Property(profile => profile.PublishedBy).HasMaxLength(450);

        builder.HasIndex(profile => new { profile.RetailerId, profile.Version })
            .IsUnique();
        builder.HasIndex(profile => new { profile.RetailerId, profile.Status })
            .IsUnique()
            .HasDatabaseName("IX_RetailerExtractionProfiles_OneDraftPerRetailer")
            .HasFilter("[Status] = N'Draft'");
        builder.HasIndex(profile => new { profile.RetailerId, profile.IsActive })
            .IsUnique()
            .HasDatabaseName("IX_RetailerExtractionProfiles_OneActivePerRetailer")
            .HasFilter("[IsActive] = 1");

        builder.HasOne(profile => profile.Retailer)
            .WithMany(retailer => retailer.ExtractionProfiles)
            .HasForeignKey(profile => profile.RetailerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(profile => profile.AllowedHosts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(profile => profile.Rules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
