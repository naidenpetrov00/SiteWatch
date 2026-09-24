using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        builder.HasIndex(listing => listing.RetailerId);
    }
}

