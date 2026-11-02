using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferProductLine : BaseEntity
{
    private OfferProductLine()
    {
    }

    public Guid OfferId { get; private set; }
    public Offer Offer { get; private set; } = null!;
    public Guid OfferProductRequirementId { get; private set; }
    public OfferProductRequirement OfferProductRequirement { get; private set; } = null!;
    public Guid ProductTypeId { get; private set; }
    public string ProductTypeName { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int ProductNumberId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public decimal? PackageQuantity { get; private set; }
    public ProductPackageUnit? PackageUnit { get; private set; }
    public decimal RequiredQuantity { get; private set; }
    public decimal OptionalQuantity { get; private set; }
    public OfferProductPriceSelection? PriceSelection { get; private set; }

    internal static OfferProductLine Create(
        Offer offer,
        OfferProductRequirement requirement,
        Product product)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(product);

        var line = new OfferProductLine
        {
            Id = Guid.NewGuid(),
            Offer = offer,
            OfferId = offer.Id,
            OfferProductRequirement = requirement,
            OfferProductRequirementId = requirement.Id
        };
        line.ReplaceProduct(requirement, product);
        return line;
    }

    internal void ReplaceProduct(
        OfferProductRequirement requirement,
        Product product)
    {
        if (requirement.Id != OfferProductRequirementId)
        {
            throw new InvalidOperationException("The product line belongs to another requirement.");
        }
        if (ProductId == product.Id)
        {
            return;
        }
        if (product.ProductTypeId != requirement.ProductTypeId)
        {
            throw new InvalidOperationException(
                "The selected product does not belong to the requirement product type.");
        }

        var changed = ProductId != Guid.Empty;
        Product = product;
        ProductId = product.Id;
        ProductTypeId = requirement.ProductTypeId;
        ProductTypeName = requirement.ProductTypeName;
        CategoryId = requirement.CategoryId;
        CategoryName = requirement.CategoryName;
        ProductNumberId = product.NumberId;
        Title = product.Title;
        Brand = product.Brand;
        Model = product.Model;
        PackageQuantity = product.PackageQuantity;
        PackageUnit = product.PackageUnit;
        SyncQuantity(requirement);
        if (changed)
        {
            ClearPriceSelection();
        }
    }

    internal void SyncQuantity(OfferProductRequirement requirement)
    {
        RequiredQuantity = requirement.IsRequired
            ? OfferQuantity.RoundCalculated(requirement.CalculatedQuantity)
            : 0m;
        OptionalQuantity = requirement.IsRequired
            ? 0m
            : OfferQuantity.RoundCalculated(requirement.CalculatedQuantity);
    }

    internal void SelectPrice(
        OfferRetailerComparison comparison,
        RetailerListing listing,
        RetailerPriceObservation observation,
        DateTimeOffset selectedAt,
        string selectedBy)
    {
        var quantity = RequiredQuantity > 0m ? RequiredQuantity : OptionalQuantity;
        _ = CalculatePriceTotal(quantity, observation.Amount, observation.Basis);
        PriceSelection = OfferProductPriceSelection.Create(
            this,
            comparison,
            listing,
            observation,
            selectedAt,
            selectedBy);
    }

    internal void ClearPriceSelection() => PriceSelection = null;

    public decimal CalculatePriceTotal(decimal quantity, decimal amount, PriceBasis basis)
    {
        var multiplier = basis switch
        {
            PriceBasis.Package when PackageQuantity.HasValue && PackageUnit.HasValue => 1m,
            PriceBasis.Item when !PackageQuantity.HasValue && !PackageUnit.HasValue => 1m,
            PriceBasis.Item when PackageQuantity.HasValue
                && PackageUnit == ProductPackageUnit.Piece =>
                PackageQuantity!.Value,
            PriceBasis.Package => throw new InvalidOperationException(
                "Package prices require product package information."),
            PriceBasis.Item => throw new InvalidOperationException(
                "Item prices require an unpackaged product or a package measured in pieces."),
            _ => throw new ArgumentOutOfRangeException(nameof(basis))
        };

        return decimal.Round(
            quantity * multiplier * amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
