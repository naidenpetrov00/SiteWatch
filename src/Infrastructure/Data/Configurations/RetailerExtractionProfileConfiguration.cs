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
                table.HasCheckConstraint(
                    "CK_RetailerExtractionProfiles_ConfigurationRevision",
                    "[ConfigurationRevision] > 0 AND "
                    + "([ValidatedConfigurationRevision] IS NULL OR "
                    + "([ValidatedConfigurationRevision] > 0 AND "
                    + "[ValidatedConfigurationRevision] <= [ConfigurationRevision]))");
                table.HasCheckConstraint(
                    "CK_RetailerExtractionProfiles_TestValidationMetadata",
                    "([ValidatedConfigurationRevision] IS NULL AND [LastSuccessfulTestAt] IS NULL "
                    + "AND [LastSuccessfulTestRuleId] IS NULL) OR "
                    + "([ValidatedConfigurationRevision] IS NOT NULL AND [LastSuccessfulTestAt] IS NOT NULL "
                    + "AND [LastSuccessfulTestRuleId] IS NOT NULL)");
            });

        builder.Property(profile => profile.Version).IsRequired();
        builder.Property(profile => profile.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(profile => profile.IsActive).IsRequired();
        builder.Property(profile => profile.PublishedBy).HasMaxLength(450);
        builder.Property(profile => profile.ConfigurationRevision)
            .IsRequired()
            .HasDefaultValue(1L)
            .IsConcurrencyToken();

        builder.HasIndex(profile => new { profile.CompanyPersonId, profile.Version })
            .IsUnique();
        builder.HasIndex(profile => new { profile.CompanyPersonId, profile.Status })
            .IsUnique()
            .HasDatabaseName("IX_RetailerExtractionProfiles_OneDraftPerCompanyPerson")
            .HasFilter("[Status] = N'Draft'");
        builder.HasIndex(profile => new { profile.CompanyPersonId, profile.IsActive })
            .IsUnique()
            .HasDatabaseName("IX_RetailerExtractionProfiles_OneActivePerCompanyPerson")
            .HasFilter("[IsActive] = 1");
        builder.HasIndex(profile => new { profile.Id, profile.LastSuccessfulTestRuleId });

        builder.HasOne(profile => profile.CompanyPerson)
            .WithMany(person => person.RetailerExtractionProfiles)
            .HasForeignKey(profile => profile.CompanyPersonId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(profile => profile.LastSuccessfulTestRule)
            .WithMany()
            .HasForeignKey(profile => new { profile.Id, profile.LastSuccessfulTestRuleId })
            .HasPrincipalKey(rule => new { rule.ExtractionProfileId, rule.Id })
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(profile => profile.AllowedHosts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(profile => profile.Rules)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
