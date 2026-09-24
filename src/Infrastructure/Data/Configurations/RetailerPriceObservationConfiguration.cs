using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RetailerPriceObservationConfiguration
    : IEntityTypeConfiguration<RetailerPriceObservation>
{
    public void Configure(EntityTypeBuilder<RetailerPriceObservation> builder)
    {
        builder.ToTable("RetailerPriceObservations");
        builder.Property(observation => observation.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(observation => observation.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(observation => observation.Basis)
            .HasConversion(
                basis => basis.ToCode(),
                value => ParseBasis(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(observation => observation.Source)
            .HasConversion(
                source => source.ToCode(),
                value => ParseSource(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(observation => observation.SourceReference)
            .HasMaxLength(RetailerPriceObservation.MaxSourceReferenceLength);
        builder.Property(observation => observation.RecordedBy)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(observation => observation.RetailerListing)
            .WithMany(listing => listing.PriceObservations)
            .HasForeignKey(observation => observation.RetailerListingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(observation => new
            {
                observation.RetailerListingId,
                observation.ObservedAt,
                observation.RecordedAt,
                observation.Id
            })
            .IsDescending(false, true, true, true);
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
