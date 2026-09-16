using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class ActivityRequirementSectionConfiguration
    : IEntityTypeConfiguration<ActivityRequirementSection>
{
    public void Configure(EntityTypeBuilder<ActivityRequirementSection> builder)
    {
        builder.ToTable("ActivityRequirementSections");

        builder.Property(section => section.Name)
            .HasMaxLength(ActivityRequirementSection.MaxNameLength);
        builder.Property(section => section.BasisQuantity)
            .HasPrecision(18, 4)
            .IsRequired();
        var measurementUnitConverter = new ValueConverter<ActivityMeasurementUnit, string>(
            unit => unit.ToCode(),
            value => ParseStoredMeasurementUnit(value));
        builder.Property(section => section.MeasurementUnit)
            .HasConversion(measurementUnitConverter)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(section => section.SortOrder).IsRequired();

        builder.HasOne(section => section.Activity)
            .WithMany(activity => activity.RequirementSections)
            .HasForeignKey(section => section.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(section => new { section.ActivityId, section.SortOrder });
    }

    private static ActivityMeasurementUnit ParseStoredMeasurementUnit(string value) =>
        ActivityMeasurementUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored activity measurement unit '{value}'.");
}
