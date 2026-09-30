using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerExtractionRuleConfiguration
    : IEntityTypeConfiguration<RetailerExtractionRule>
{
    public void Configure(EntityTypeBuilder<RetailerExtractionRule> builder)
    {
        builder.ToTable(
            "RetailerExtractionRules",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_RetailerExtractionRules_Priority",
                    "[Priority] > 0");
                table.HasCheckConstraint(
                    "CK_RetailerExtractionRules_AcceptableRange",
                    "([MinimumValue] IS NULL OR [MinimumValue] > 0) AND "
                    + "([MaximumValue] IS NULL OR [MaximumValue] > 0) AND "
                    + "([MinimumValue] IS NULL OR [MaximumValue] IS NULL OR [MinimumValue] <= [MaximumValue])");
                table.HasCheckConstraint(
                    "CK_RetailerExtractionRules_EurCurrency",
                    "[ExpectedCurrencyCode] = N'EUR'");
            });

        builder.Property(rule => rule.Name)
            .HasMaxLength(RetailerExtractionRule.MaxNameLength)
            .IsRequired();
        builder.Property(rule => rule.IsEnabled).IsRequired();
        builder.Property(rule => rule.Priority).IsRequired();
        builder.Property(rule => rule.RuleType)
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired();
        builder.Property(rule => rule.JsonLdObjectType)
            .HasMaxLength(RetailerExtractionRule.MaxObjectTypeLength);
        builder.Property(rule => rule.JsonLdPricePath)
            .HasMaxLength(RetailerExtractionRule.MaxPathLength);
        builder.Property(rule => rule.JsonLdCurrencyPath)
            .HasMaxLength(RetailerExtractionRule.MaxPathLength);
        builder.Property(rule => rule.CssSelector)
            .HasMaxLength(RetailerExtractionRule.MaxSelectorLength);
        builder.Property(rule => rule.CssValueSource)
            .HasConversion<string>()
            .HasMaxLength(24);
        builder.Property(rule => rule.CssAttributeName)
            .HasMaxLength(RetailerExtractionRule.MaxAttributeNameLength);
        builder.Property(rule => rule.DecimalSeparator)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(rule => rule.ThousandsSeparator)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(rule => rule.ExpectedCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(rule => rule.PriceBasis)
            .HasConversion(
                basis => basis.ToCode(),
                value => ParseBasis(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(rule => rule.MinimumValue).HasPrecision(18, 2);
        builder.Property(rule => rule.MaximumValue).HasPrecision(18, 2);

        builder.HasAlternateKey(rule => new { rule.ExtractionProfileId, rule.Id });
        builder.HasIndex(rule => new { rule.ExtractionProfileId, rule.Priority })
            .IsUnique();
        builder.HasOne(rule => rule.ExtractionProfile)
            .WithMany(profile => profile.Rules)
            .HasForeignKey(rule => rule.ExtractionProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static PriceBasis ParseBasis(string value) =>
        PriceBasisCodes.TryParse(value, out var basis)
            ? basis
            : throw new InvalidOperationException(
                $"Unsupported stored price basis '{value}'.");
}
