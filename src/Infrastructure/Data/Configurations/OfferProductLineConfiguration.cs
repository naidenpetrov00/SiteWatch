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
        builder.Property(line => line.Brand).HasMaxLength(100);
        builder.Property(line => line.Model).HasMaxLength(100);
        builder.Property(line => line.PackageQuantity).HasPrecision(18, 4);
        builder.Property(line => line.RequiredQuantity)
            .HasPrecision(28, 8)
            .IsRequired();
        builder.Property(line => line.OptionalQuantity)
            .HasPrecision(28, 8)
            .IsRequired();

        var categoryConverter = new ValueConverter<ProductCategory, string>(
            category => category.ToCode(),
            value => ParseCategory(value));
        builder.Property(line => line.Category)
            .HasConversion(categoryConverter)
            .HasMaxLength(100)
            .IsRequired();
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
        builder.HasOne(line => line.Product)
            .WithMany()
            .HasForeignKey(line => line.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(line => new { line.OfferId, line.ProductId }).IsUnique();
        builder.HasIndex(line => new { line.OfferId, line.ProductNumberId });
    }

    private static ProductCategory ParseCategory(string value) =>
        ProductCategoryCodes.TryParse(value, out var category)
            ? category
            : throw new InvalidOperationException(
                $"Unsupported stored product category '{value}'.");

    private static ProductPackageUnit ParsePackageUnit(string value) =>
        ProductPackageUnitCodes.TryParse(value, out var unit)
            ? unit
            : throw new InvalidOperationException(
                $"Unsupported stored product package unit '{value}'.");
}
