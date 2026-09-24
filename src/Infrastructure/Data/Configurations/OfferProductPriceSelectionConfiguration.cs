using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class OfferProductPriceSelectionConfiguration
    : IEntityTypeConfiguration<OfferProductPriceSelection>
{
    public void Configure(EntityTypeBuilder<OfferProductPriceSelection> builder)
    {
        builder.ToTable("OfferProductPriceSelections");
        builder.Property(selection => selection.RetailerDisplayName)
            .HasMaxLength(Retailer.MaxDisplayNameLength)
            .IsRequired();
        builder.Property(selection => selection.ProductUrl)
            .HasMaxLength(RetailerListing.MaxProductUrlLength);
        builder.Property(selection => selection.RetailerProductCode)
            .HasMaxLength(RetailerListing.MaxRetailerProductCodeLength);
        builder.Property(selection => selection.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(selection => selection.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(selection => selection.Basis)
            .HasConversion(
                basis => basis.ToCode(),
                value => ParseBasis(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(selection => selection.Source)
            .HasConversion(
                source => source.ToCode(),
                value => ParseSource(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(selection => selection.SourceReference)
            .HasMaxLength(RetailerPriceObservation.MaxSourceReferenceLength);
        builder.Property(selection => selection.RecordedBy)
            .HasMaxLength(450)
            .IsRequired();
        builder.Property(selection => selection.SelectedBy)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(selection => selection.OfferProductLine)
            .WithOne(line => line.PriceSelection)
            .HasForeignKey<OfferProductPriceSelection>(selection => selection.OfferProductLineId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Retailer>()
            .WithMany()
            .HasForeignKey(selection => selection.RetailerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RetailerListing>()
            .WithMany()
            .HasForeignKey(selection => selection.RetailerListingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RetailerPriceObservation>()
            .WithMany()
            .HasForeignKey(selection => selection.RetailerPriceObservationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(selection => selection.RetailerListingId);
        builder.HasIndex(selection => selection.RetailerPriceObservationId);
        builder.HasIndex(selection => selection.RetailerId);
    }

    private static PriceBasis ParseBasis(string value) =>
        PriceBasisCodes.TryParse(value, out var basis)
            ? basis
            : throw new InvalidOperationException($"Unsupported stored price basis '{value}'.");

    private static PriceObservationSource ParseSource(string value) =>
        value.Trim().ToLowerInvariant() switch
        {
            "manual" => PriceObservationSource.Manual,
            "automated" => PriceObservationSource.Automated,
            _ => throw new InvalidOperationException(
                $"Unsupported stored price observation source '{value}'.")
        };
}
