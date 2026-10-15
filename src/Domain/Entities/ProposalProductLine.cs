using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class ProposalProductLine : BaseEntity
{
    private ProposalProductLine()
    {
    }

    public Guid ProposalId { get; private set; }
    public Proposal Proposal { get; private set; } = null!;
    public Guid SourceOfferProductLineId { get; private set; }
    public Guid ProductId { get; private set; }
    public int ProductNumberId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ProductCategory Category { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public decimal? PackageQuantity { get; private set; }
    public ProductPackageUnit? PackageUnit { get; private set; }
    public decimal RequiredQuantity { get; private set; }
    public decimal OptionalQuantity { get; private set; }
    public Guid? SelectedRetailerId { get; private set; }
    public string? SelectedRetailerDisplayName { get; private set; }
    public decimal? SelectedPriceAmount { get; private set; }
    public string? SelectedPriceCurrencyCode { get; private set; }
    public PriceBasis? SelectedPriceBasis { get; private set; }
    public decimal? RequiredTotal { get; private set; }
    public decimal? OptionalTotal { get; private set; }
    public bool IsOptionalPriceExcluded { get; private set; }
    public int SortOrder { get; private set; }

    internal static ProposalProductLine Create(
        Proposal proposal,
        OfferProductLine source,
        int sortOrder)
    {
        var selection = source.PriceSelection;
        if (source.RequiredQuantity > 0m && selection is null)
        {
            throw new InvalidOperationException(
                "Every required product requires a selected price before creating a Proposal.");
        }

        if (selection is not null
            && (selection.CurrencyCode != RetailerPriceObservation.EuroCurrencyCode
                || selection.Amount <= 0m))
        {
            throw new InvalidOperationException(
                "Every selected Proposal price must be a valid positive EUR amount.");
        }

        decimal? requiredTotal = null;
        decimal? optionalTotal = null;
        if (selection is not null)
        {
            if (source.RequiredQuantity > 0m)
            {
                requiredTotal = source.CalculatePriceTotal(
                    source.RequiredQuantity,
                    selection.Amount,
                    selection.Basis);
                if (requiredTotal.Value <= 0m)
                {
                    throw new InvalidOperationException(
                        "Every required product requires a valid calculated total.");
                }
            }

            if (source.OptionalQuantity > 0m)
            {
                optionalTotal = source.CalculatePriceTotal(
                    source.OptionalQuantity,
                    selection.Amount,
                    selection.Basis);
                if (optionalTotal.Value <= 0m)
                {
                    throw new InvalidOperationException(
                        "Every priced optional product requires a valid calculated total.");
                }
            }
        }

        return new ProposalProductLine
        {
            Id = Guid.NewGuid(),
            Proposal = proposal,
            ProposalId = proposal.Id,
            SourceOfferProductLineId = source.Id,
            ProductId = source.ProductId,
            ProductNumberId = source.ProductNumberId,
            Title = source.Title,
            Category = source.Category,
            Brand = source.Brand,
            Model = source.Model,
            PackageQuantity = source.PackageQuantity,
            PackageUnit = source.PackageUnit,
            RequiredQuantity = source.RequiredQuantity,
            OptionalQuantity = source.OptionalQuantity,
            SelectedRetailerId = selection?.RetailerId,
            SelectedRetailerDisplayName = selection?.RetailerDisplayName,
            SelectedPriceAmount = selection?.Amount,
            SelectedPriceCurrencyCode = selection?.CurrencyCode,
            SelectedPriceBasis = selection?.Basis,
            RequiredTotal = requiredTotal,
            OptionalTotal = optionalTotal,
            IsOptionalPriceExcluded = source.RequiredQuantity == 0m
                && source.OptionalQuantity > 0m
                && selection is null,
            SortOrder = sortOrder
        };
    }
}
