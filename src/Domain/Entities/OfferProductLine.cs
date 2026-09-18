using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferProductLine : BaseEntity
{
    private readonly List<OfferProductContribution> _contributions = [];

    private OfferProductLine()
    {
    }

    public Guid OfferId { get; private set; }
    public Offer Offer { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int ProductNumberId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ProductCategory Category { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public decimal? PackageQuantity { get; private set; }
    public ProductPackageUnit? PackageUnit { get; private set; }
    public decimal RequiredQuantity { get; private set; }
    public decimal OptionalQuantity { get; private set; }
    public IReadOnlyCollection<OfferProductContribution> Contributions => _contributions;

    public static OfferProductLine Create(Offer offer, Product product)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(product);

        return new OfferProductLine
        {
            Id = Guid.NewGuid(),
            Offer = offer,
            OfferId = offer.Id,
            Product = product,
            ProductId = product.Id,
            ProductNumberId = product.NumberId,
            Title = product.Title,
            Category = product.Category,
            Brand = product.Brand,
            Model = product.Model,
            PackageQuantity = product.PackageQuantity,
            PackageUnit = product.PackageUnit
        };
    }

    public OfferProductContribution AddContribution(
        OfferActivitySection section,
        ActivityProductRequirement sourceRequirement)
    {
        if (sourceRequirement.ProductId != ProductId)
        {
            throw new InvalidOperationException("The requirement references another product.");
        }

        var contribution = OfferProductContribution.Create(
            section,
            this,
            sourceRequirement);
        _contributions.Add(contribution);
        section.AddContribution(contribution);
        RecalculateTotals();
        return contribution;
    }

    public void RemoveContribution(OfferProductContribution contribution)
    {
        if (!_contributions.Remove(contribution))
        {
            throw new InvalidOperationException("The contribution does not belong to this product line.");
        }

        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        RequiredQuantity = OfferQuantity.RoundCalculated(
            _contributions
                .Where(contribution => contribution.IsRequired)
                .Sum(contribution => contribution.CalculatedQuantity));
        OptionalQuantity = OfferQuantity.RoundCalculated(
            _contributions
                .Where(contribution => !contribution.IsRequired)
                .Sum(contribution => contribution.CalculatedQuantity));
    }
}
