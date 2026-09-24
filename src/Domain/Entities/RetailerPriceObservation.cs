using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class RetailerPriceObservation : BaseEntity
{
    public const string EuroCurrencyCode = "EUR";
    public const int MaxSourceReferenceLength = 500;

    private RetailerPriceObservation()
    {
    }

    public Guid RetailerListingId { get; private set; }
    public RetailerListing RetailerListing { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = EuroCurrencyCode;
    public PriceBasis Basis { get; private set; }
    public DateTimeOffset ObservedAt { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public PriceObservationSource Source { get; private set; }
    public string? SourceReference { get; private set; }
    public string RecordedBy { get; private set; } = string.Empty;

    internal static RetailerPriceObservation Create(
        RetailerListing listing,
        decimal amount,
        PriceBasis basis,
        DateTimeOffset observedAt,
        DateTimeOffset recordedAt,
        PriceObservationSource source,
        string? sourceReference,
        string recordedBy)
    {
        ArgumentNullException.ThrowIfNull(listing);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordedBy);
        if (amount <= 0m || decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Price must be positive and have at most two decimal places.");
        }

        if (!Enum.IsDefined(basis))
        {
            throw new ArgumentOutOfRangeException(nameof(basis));
        }

        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        var normalizedSourceReference = string.IsNullOrWhiteSpace(sourceReference)
            ? null
            : sourceReference.Trim();
        if (normalizedSourceReference?.Length > MaxSourceReferenceLength)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceReference));
        }

        return new RetailerPriceObservation
        {
            Id = Guid.NewGuid(),
            RetailerListing = listing,
            RetailerListingId = listing.Id,
            Amount = amount,
            CurrencyCode = EuroCurrencyCode,
            Basis = basis,
            ObservedAt = observedAt,
            RecordedAt = recordedAt,
            Source = source,
            SourceReference = normalizedSourceReference,
            RecordedBy = recordedBy.Trim()
        };
    }
}
