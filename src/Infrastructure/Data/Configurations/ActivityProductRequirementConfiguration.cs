using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class ActivityProductRequirementConfiguration
    : IEntityTypeConfiguration<ActivityProductRequirement>
{
    public void Configure(EntityTypeBuilder<ActivityProductRequirement> builder)
    {
        builder.ToTable("ActivityProductRequirements");

        builder.Property(requirement => requirement.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();
        builder.Property(requirement => requirement.IsRequired).IsRequired();
        var quantityBehaviorConverter = new ValueConverter<ProductQuantityBehavior, string>(
            behavior => behavior.ToCode(),
            value => ParseStoredQuantityBehavior(value));
        builder.Property(requirement => requirement.QuantityBehavior)
            .HasConversion(quantityBehaviorConverter)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(requirement => requirement.Notes)
            .HasMaxLength(ActivityProductRequirement.MaxNotesLength);
        builder.Property(requirement => requirement.SortOrder).IsRequired();

        builder.HasOne(requirement => requirement.Section)
            .WithMany(section => section.ProductRequirements)
            .HasForeignKey(requirement => requirement.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(requirement => requirement.Product)
            .WithMany()
            .HasForeignKey(requirement => requirement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(requirement => new
            {
                requirement.SectionId,
                requirement.ProductId
            })
            .IsUnique();
        builder.HasIndex(requirement => new
            {
                requirement.SectionId,
                requirement.SortOrder
            });
    }

    private static ProductQuantityBehavior ParseStoredQuantityBehavior(string value) =>
        ProductQuantityBehaviorCodes.TryParse(value, out var behavior)
            ? behavior
            : throw new InvalidOperationException(
                $"Unsupported stored product quantity behavior '{value}'.");
}
