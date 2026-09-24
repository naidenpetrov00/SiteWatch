using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class OfferRetailerComparisonConfiguration
    : IEntityTypeConfiguration<OfferRetailerComparison>
{
    public void Configure(EntityTypeBuilder<OfferRetailerComparison> builder)
    {
        builder.ToTable("OfferRetailerComparisons");
        builder.Property(comparison => comparison.RetailerDisplayName)
            .HasMaxLength(Retailer.MaxDisplayNameLength)
            .IsRequired();
        builder.Property(comparison => comparison.RetailerBaseWebsiteUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.HasOne(comparison => comparison.Offer)
            .WithMany(offer => offer.RetailerComparisons)
            .HasForeignKey(comparison => comparison.OfferId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(comparison => comparison.Retailer)
            .WithMany()
            .HasForeignKey(comparison => comparison.RetailerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(comparison => new { comparison.OfferId, comparison.RetailerId })
            .IsUnique();
    }
}

