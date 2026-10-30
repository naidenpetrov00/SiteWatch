using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class OfferProductLineConfiguration
    : IEntityTypeConfiguration<OfferProductLine>
{
    public void Configure(EntityTypeBuilder<OfferProductLine> builder)
    {
        builder.ToTable("OfferProductLines");

        builder.Property(line => line.ProductNumberId).IsRequired();
        builder.Property(line => line.Title).HasMaxLength(200).IsRequired();
        builder.Property(line => line.ProductTypeName)
            .HasMaxLength(ProductType.MaxNameLength)
            .IsRequired();
        builder.Property(line => line.CategoryName)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();
        builder.Property(line => line.Brand).HasMaxLength(100);
        builder.Property(line => line.Model).HasMaxLength(100);
        builder.Property(line => line.PackageQuantity).HasPrecision(18, 4);
        builder.Property(line => line.RequiredQuantity).HasPrecision(28, 8).IsRequired();
        builder.Property(line => line.OptionalQuantity).HasPrecision(28, 8).IsRequired();

        var packageUnitConverter = new ValueConverter<ProductPackageUnit, string>(
            unit => unit.ToCode(),
            value => ParsePackageUnit(value));
        builder.Property(line => line.PackageUnit)
            .HasConversion(packageUnitConverter)
            .HasMaxLength(50);

        builder.HasOne(line => line.Offer)
            .WithMany(offer => offer.ProductLines)
            .HasForeignKey(line => line.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(line => line.OfferProductRequirement)
            .WithOne(requirement => requirement.ProductLine)
            .HasForeignKey<OfferProductLine>(line => new
            {
                line.OfferProductRequirementId,
                line.ProductTypeId
            })
            .HasPrincipalKey<OfferProductRequirement>(requirement => new
            {
                requirement.Id,
                requirement.ProductTypeId
            })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(line => line.Product)
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(line => line.OfferProductRequirementId).IsUnique();
        builder.HasIndex(line => new { line.OfferId, line.ProductNumberId });
        builder.HasIndex(line => line.ProductId);
    }

    private static ProductPackageUnit ParsePackageUnit(string value) =>
        ProductPackageUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored product package unit '{value}'.");
}
