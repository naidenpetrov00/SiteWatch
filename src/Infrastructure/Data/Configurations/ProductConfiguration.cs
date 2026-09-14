using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.Property(product => product.NumberId)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEXT VALUE FOR [dbo].[ProductNumberIds]")
            .IsRequired();

        builder.Property(product => product.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000);
        builder.Property(product => product.Brand).HasMaxLength(100);
        builder.Property(product => product.Model).HasMaxLength(100);
        builder.Property(product => product.PackageQuantity).HasPrecision(18, 4);
        var packageUnitConverter = new ValueConverter<ProductPackageUnit, string>(
            unit => unit.ToCode(),
            value => ParseStoredPackageUnit(value));
        builder.Property(product => product.PackageUnit)
            .HasConversion(packageUnitConverter)
            .HasMaxLength(50);
        builder.Property(product => product.Category)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(product => product.SearchIdentity)
            .HasMaxLength(1200)
            .IsRequired();

        var searchConfigurationConverter = new ValueConverter<ProductSearchConfiguration, string>(
            configuration => configuration.ToStorageValue(),
            value => ProductSearchConfiguration.FromStorageValue(value));
        var searchConfigurationComparer = new ValueComparer<ProductSearchConfiguration>(
            (left, right) => EqualityComparer<ProductSearchConfiguration>.Default.Equals(left, right),
            configuration => configuration.GetHashCode(),
            configuration => ProductSearchConfiguration.FromStorageValue(
                configuration.ToStorageValue()));

        builder.Property(product => product.SearchConfiguration)
            .HasColumnType("nvarchar(max)")
            .HasConversion(searchConfigurationConverter)
            .IsRequired()
            .Metadata.SetValueComparer(searchConfigurationComparer);

        builder.HasIndex(product => product.NumberId).IsUnique();
        builder.HasIndex(product => product.Title);
        builder.HasIndex(product => product.Category);
        builder.HasIndex(product => product.Status);
    }

    private static ProductPackageUnit ParseStoredPackageUnit(string value)
    {
        if (ProductPackageUnitCodes.TryParse(value, out var unit))
        {
            return unit;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "pc" or "pcs" or "pieces" => ProductPackageUnit.Piece,
            "kilogram" or "kilograms" => ProductPackageUnit.Kilogram,
            "gram" or "grams" => ProductPackageUnit.Gram,
            "liter" or "liters" or "litre" or "litres" => ProductPackageUnit.Liter,
            "milliliter" or "milliliters" or "millilitre" or "millilitres" =>
                ProductPackageUnit.Milliliter,
            "meter" or "meters" or "metre" or "metres" => ProductPackageUnit.Meter,
            "centimeter" or "centimeters" or "centimetre" or "centimetres" =>
                ProductPackageUnit.Centimeter,
            _ => throw new InvalidOperationException(
                $"Unsupported stored product package unit '{value}'.")
        };
    }
}
