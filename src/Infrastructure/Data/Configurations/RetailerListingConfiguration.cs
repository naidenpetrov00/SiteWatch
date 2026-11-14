using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerListingConfiguration
    : IEntityTypeConfiguration<RetailerListing>
{
    public void Configure(EntityTypeBuilder<RetailerListing> builder)
    {
        builder.ToTable("RetailerListings");
        builder.Property(listing => listing.ProductUrl)
            .HasMaxLength(RetailerListing.MaxProductUrlLength);
        builder.Property(listing => listing.RetailerProductCode)
            .HasMaxLength(RetailerListing.MaxRetailerProductCodeLength);
        var onlinePriceBasisConverter = new ValueConverter<PriceBasis?, string?>(
            basis => basis.HasValue ? basis.Value.ToCode() : null,
            value => value == null ? null : ParseBasis(value));
        builder.Property(listing => listing.OnlinePriceBasis)
            .HasConversion(onlinePriceBasisConverter)
            .HasMaxLength(16);
        builder.Property(listing => listing.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(listing => listing.Product)
            .WithMany()
            .HasForeignKey(listing => listing.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(listing => listing.Retailer)
            .WithMany()
            .HasForeignKey(listing => listing.RetailerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(listing => new { listing.ProductId, listing.RetailerId })
            .IsUnique();
        builder.HasIndex(listing => new { listing.ProductId, listing.IsActive });
        builder.HasIndex(listing => new { listing.RetailerId, listing.IsActive });
    }

    private static PriceBasis ParseBasis(string value) =>
        PriceBasisCodes.TryParse(value, out var basis)
            ? basis
            : throw new InvalidOperationException(
                $"Unsupported stored online price basis '{value}'.");
}

