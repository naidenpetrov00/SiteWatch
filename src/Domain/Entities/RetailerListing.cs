using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class RetailerListing : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxProductUrlLength = 2048;
    public const int MaxRetailerProductCodeLength = 100;

    private readonly List<RetailerPriceObservation> _priceObservations = [];

    private RetailerListing()
    {
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid RetailerId { get; private set; }
    public Retailer Retailer { get; private set; } = null!;
    public string? ProductUrl { get; private set; }
    public bool IsActive { get; private set; }
    public string? RetailerProductCode { get; private set; }
    public IReadOnlyCollection<RetailerPriceObservation> PriceObservations =>
        _priceObservations;

    public static RetailerListing Create(
        Product product,
        Retailer retailer,
        string? productUrl,
        string? retailerProductCode)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(retailer);

        var listing = new RetailerListing
        {
            Id = Guid.NewGuid(),
            Product = product,
            ProductId = product.Id,
            Retailer = retailer,
            RetailerId = retailer.Id,
            IsActive = true
        };
        listing.UpdateMetadata(productUrl, retailerProductCode);
        return listing;
    }

    public void UpdateMetadata(string? productUrl, string? retailerProductCode)
    {
        ProductUrl = NormalizeProductUrl(productUrl);
        RetailerProductCode = NormalizeOptional(
            retailerProductCode,
            MaxRetailerProductCodeLength,
            nameof(retailerProductCode));
    }

    public RetailerPriceObservation RecordPrice(
        decimal amount,
        PriceBasis basis,
        DateTimeOffset observedAt,
        DateTimeOffset recordedAt,
        PriceObservationSource source,
        string? sourceReference,
        string recordedBy)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "Prices cannot be recorded for an inactive retailer listing.");
        }

        var observation = RetailerPriceObservation.Create(
            this,
            amount,
            basis,
            observedAt,
            recordedAt,
            source,
            sourceReference,
            recordedBy);
        _priceObservations.Add(observation);
        return observation;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string? NormalizeProductUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var candidate = value.Trim();
        if (candidate.Length > MaxProductUrlLength
            || !Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrWhiteSpace(uri.Host)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "The retailer product URL must be an absolute HTTP or HTTPS URL without credentials or a fragment.",
                nameof(value));
        }

        return uri.AbsoluteUri;
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = string.Join(
            " ",
            value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return normalized;
    }
}
