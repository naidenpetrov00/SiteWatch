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
    public Guid? ExtractionProfileId { get; private set; }
    public RetailerExtractionProfile? ExtractionProfile { get; private set; }
    public Guid? MatchedExtractionRuleId { get; private set; }
    public RetailerExtractionRule? MatchedExtractionRule { get; private set; }

    internal static RetailerPriceObservation Create(
        RetailerListing listing,
        decimal amount,
        PriceBasis basis,
        DateTimeOffset observedAt,
        DateTimeOffset recordedAt,
        PriceObservationSource source,
        string? sourceReference,
        string recordedBy,
        RetailerExtractionProfile? extractionProfile = null,
        RetailerExtractionRule? matchedRule = null)
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

        if ((extractionProfile is null) != (matchedRule is null))
        {
            throw new ArgumentException(
                "Extraction profile and matched rule provenance must be supplied together.");
        }
        if (source == PriceObservationSource.Manual && extractionProfile is not null)
        {
            throw new ArgumentException("Manual observations cannot contain extraction provenance.");
        }
        if (source == PriceObservationSource.Automated && extractionProfile is null)
        {
            throw new ArgumentException("Automated observations require extraction provenance.");
        }
        if (extractionProfile is not null
            && (extractionProfile.Status != RetailerExtractionProfileStatus.Published
                || extractionProfile.CompanyPersonId != listing.Retailer.CompanyPersonId
                || matchedRule!.ExtractionProfileId != extractionProfile.Id))
        {
            throw new ArgumentException(
                "The matched extraction rule must belong to a published profile for the listing retailer's company Person.");
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
            RecordedBy = recordedBy.Trim(),
            ExtractionProfile = extractionProfile,
            ExtractionProfileId = extractionProfile?.Id,
            MatchedExtractionRule = matchedRule,
            MatchedExtractionRuleId = matchedRule?.Id
        };
    }
}
