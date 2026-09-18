using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class OfferActivitySectionConfiguration
    : IEntityTypeConfiguration<OfferActivitySection>
{
    public void Configure(EntityTypeBuilder<OfferActivitySection> builder)
    {
        builder.ToTable("OfferActivitySections");

        builder.Property(section => section.Name)
            .HasMaxLength(ActivityRequirementSection.MaxNameLength);
        builder.Property(section => section.BasisQuantity)
            .HasPrecision(18, 4)
            .IsRequired();
        builder.Property(section => section.RequestedMeasurement)
            .HasPrecision(18, 4)
            .IsRequired();
        var measurementUnitConverter = new ValueConverter<ActivityMeasurementUnit, string>(
            unit => unit.ToCode(),
            value => ParseMeasurementUnit(value));
        builder.Property(section => section.MeasurementUnit)
            .HasConversion(measurementUnitConverter)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(section => section.SortOrder).IsRequired();

        builder.HasOne(section => section.OfferActivity)
            .WithMany(activity => activity.Sections)
            .HasForeignKey(section => section.OfferActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(section => new
            {
                section.OfferActivityId,
                section.SourceSectionId
            })
            .IsUnique();
        builder.HasIndex(section => new { section.OfferActivityId, section.SortOrder });
    }

    private static ActivityMeasurementUnit ParseMeasurementUnit(string value) =>
        ActivityMeasurementUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored activity measurement unit '{value}'.");
}
