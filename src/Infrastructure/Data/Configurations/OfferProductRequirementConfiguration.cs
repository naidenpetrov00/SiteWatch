using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class OfferProductRequirementConfiguration
    : IEntityTypeConfiguration<OfferProductRequirement>
{
    public void Configure(EntityTypeBuilder<OfferProductRequirement> builder)
    {
        builder.ToTable("OfferProductRequirements");
        builder.Property(requirement => requirement.ProductTypeName)
            .HasMaxLength(ProductType.MaxNameLength)
            .IsRequired();
        builder.Property(requirement => requirement.CategoryName)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();
        builder.Property(requirement => requirement.ConfiguredQuantity)
            .HasPrecision(18, 4)
            .IsRequired();
        builder.Property(requirement => requirement.CalculatedQuantity)
            .HasPrecision(28, 8)
            .IsRequired();
        builder.Property(requirement => requirement.IsRequired).IsRequired();
        var behaviorConverter = new ValueConverter<ProductQuantityBehavior, string>(
            behavior => behavior.ToCode(),
            value => ParseQuantityBehavior(value));
        builder.Property(requirement => requirement.QuantityBehavior)
            .HasConversion(behaviorConverter)
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(requirement => requirement.Notes)
            .HasMaxLength(ActivityProductRequirement.MaxNotesLength);
        builder.Property(requirement => requirement.SortOrder).IsRequired();

        builder.HasAlternateKey(requirement => new { requirement.Id, requirement.ProductTypeId });
        builder.HasOne(requirement => requirement.OfferActivitySection)
            .WithMany(section => section.ProductRequirements)
            .HasForeignKey(requirement => requirement.OfferActivitySectionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProductType>()
            .WithMany()
            .HasForeignKey(requirement => requirement.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(requirement => requirement.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(requirement => new
        {
            requirement.OfferActivitySectionId,
            requirement.SourceRequirementId
        }).IsUnique();
        builder.HasIndex(requirement => new
        {
            requirement.OfferActivitySectionId,
            requirement.SortOrder
        });
    }

    private static ProductQuantityBehavior ParseQuantityBehavior(string value) =>
        ProductQuantityBehaviorCodes.TryParse(value, out var behavior)
            ? behavior
            : throw new InvalidOperationException(
                $"Unsupported stored product quantity behavior '{value}'.");
}
