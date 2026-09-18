using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferProductContribution : BaseEntity
{
    private OfferProductContribution()
    {
    }

    public Guid OfferActivitySectionId { get; private set; }
    public OfferActivitySection OfferActivitySection { get; private set; } = null!;
    public Guid OfferProductLineId { get; private set; }
    public OfferProductLine OfferProductLine { get; private set; } = null!;
    public Guid SourceRequirementId { get; private set; }
    public decimal ConfiguredQuantity { get; private set; }
    public bool IsRequired { get; private set; }
    public ProductQuantityBehavior QuantityBehavior { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }
    public decimal CalculatedQuantity { get; private set; }

    internal static OfferProductContribution Create(
        OfferActivitySection section,
        OfferProductLine productLine,
        ActivityProductRequirement sourceRequirement)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(productLine);
        ArgumentNullException.ThrowIfNull(sourceRequirement);

        var contribution = new OfferProductContribution
        {
            Id = Guid.NewGuid(),
            OfferActivitySection = section,
            OfferActivitySectionId = section.Id,
            OfferProductLine = productLine,
            OfferProductLineId = productLine.Id,
            SourceRequirementId = sourceRequirement.Id,
            ConfiguredQuantity = sourceRequirement.Quantity,
            IsRequired = sourceRequirement.IsRequired,
            QuantityBehavior = sourceRequirement.QuantityBehavior,
            Notes = sourceRequirement.Notes,
            SortOrder = sourceRequirement.SortOrder
        };
        contribution.Recalculate(section.RequestedMeasurement, section.BasisQuantity);
        return contribution;
    }

    internal void Recalculate(decimal requestedMeasurement, decimal basisQuantity)
    {
        CalculatedQuantity = QuantityBehavior == ProductQuantityBehavior.Fixed
            ? OfferQuantity.RoundCalculated(ConfiguredQuantity)
            : OfferQuantity.CalculateProportional(
                ConfiguredQuantity,
                requestedMeasurement,
                basisQuantity);
    }
}
