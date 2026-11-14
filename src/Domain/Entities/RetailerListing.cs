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
    public PriceBasis? OnlinePriceBasis { get; private set; }
    public IReadOnlyCollection<RetailerPriceObservation> PriceObservations =>
        _priceObservations;

    public static RetailerListing Create(
        Product product,
        Retailer retailer,
        string? productUrl,
        string? retailerProductCode,
        PriceBasis? onlinePriceBasis = null)
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
        listing.UpdateMetadata(productUrl, retailerProductCode, onlinePriceBasis);
        return listing;
    }

    public void UpdateMetadata(
        string? productUrl,
        string? retailerProductCode,
        PriceBasis? onlinePriceBasis = null)
    {
        var normalizedProductUrl = NormalizeProductUrl(productUrl);
        ValidateOnlinePriceBasis(Product, normalizedProductUrl, onlinePriceBasis);

        ProductUrl = normalizedProductUrl;
        RetailerProductCode = NormalizeOptional(
            retailerProductCode,
            MaxRetailerProductCodeLength,
            nameof(retailerProductCode));
        OnlinePriceBasis = onlinePriceBasis;
    }

    public bool HasValidOnlinePriceBasis() =>
        OnlinePriceBasis.HasValue && Product.SupportsPriceBasis(OnlinePriceBasis.Value);

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

    public RetailerPriceObservation RecordAutomatedPrice(
        decimal amount,
        DateTimeOffset observedAt,
        DateTimeOffset recordedAt,
        string sourceReference,
        string recordedBy,
        RetailerExtractionProfile extractionProfile,
        RetailerExtractionRule matchedRule)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "Prices cannot be recorded for an inactive retailer listing.");
        }
        if (!HasValidOnlinePriceBasis())
        {
            throw new InvalidOperationException(
                "Automated prices require a valid online price basis on the retailer listing.");
        }

        var observation = RetailerPriceObservation.Create(
            this,
            amount,
            OnlinePriceBasis!.Value,
            observedAt,
            recordedAt,
            PriceObservationSource.Automated,
            sourceReference,
            recordedBy,
            extractionProfile,
            matchedRule);
        _priceObservations.Add(observation);
        return observation;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void ValidateOnlinePriceBasis(
        Product product,
        string? productUrl,
        PriceBasis? onlinePriceBasis)
    {
        if (!string.IsNullOrWhiteSpace(productUrl) && !onlinePriceBasis.HasValue)
        {
            throw new ArgumentException(
                "An online price basis is required when a retailer product URL is supplied.",
                nameof(onlinePriceBasis));
        }

        if (onlinePriceBasis.HasValue && !product.SupportsPriceBasis(onlinePriceBasis.Value))
        {
            throw new ArgumentException(
                product.PackageQuantity.HasValue
                    ? "Item pricing is supported only for packages measured in pieces."
                    : "Package pricing requires Product package metadata.",
                nameof(onlinePriceBasis));
        }
    }

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
