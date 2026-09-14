using Domain.Entities;
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
        builder.Property(product => product.PackageUnit).HasMaxLength(50);
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
}
