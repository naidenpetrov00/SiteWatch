using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class OfferProductContributionConfiguration
    : IEntityTypeConfiguration<OfferProductContribution>
{
    public void Configure(EntityTypeBuilder<OfferProductContribution> builder)
    {
        builder.ToTable("OfferProductContributions");

        builder.Property(contribution => contribution.ConfiguredQuantity)
            .HasPrecision(18, 4)
            .IsRequired();
        builder.Property(contribution => contribution.CalculatedQuantity)
            .HasPrecision(28, 8)
            .IsRequired();
        builder.Property(contribution => contribution.IsRequired).IsRequired();
        var behaviorConverter = new ValueConverter<ProductQuantityBehavior, string>(
            behavior => behavior.ToCode(),
            value => ParseQuantityBehavior(value));
        builder.Property(contribution => contribution.QuantityBehavior)
            .HasConversion(behaviorConverter)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(contribution => contribution.Notes)
            .HasMaxLength(ActivityProductRequirement.MaxNotesLength);
        builder.Property(contribution => contribution.SortOrder).IsRequired();

        builder.HasOne(contribution => contribution.OfferActivitySection)
            .WithMany(section => section.ProductContributions)
            .HasForeignKey(contribution => contribution.OfferActivitySectionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(contribution => contribution.OfferProductLine)
            .WithMany(line => line.Contributions)
            .HasForeignKey(contribution => contribution.OfferProductLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(contribution => new
            {
                contribution.OfferActivitySectionId,
                contribution.SourceRequirementId
            })
            .IsUnique();
        builder.HasIndex(contribution => new
            {
                contribution.OfferActivitySectionId,
                contribution.SortOrder
            });
        builder.HasIndex(contribution => contribution.OfferProductLineId);
    }

    private static ProductQuantityBehavior ParseQuantityBehavior(string value) =>
        ProductQuantityBehaviorCodes.TryParse(value, out var behavior)
            ? behavior
            : throw new InvalidOperationException(
                $"Unsupported stored product quantity behavior '{value}'.");
}
