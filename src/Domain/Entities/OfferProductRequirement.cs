using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class OfferProductRequirement : BaseEntity
{
    private OfferProductRequirement()
    {
    }

    public Guid OfferActivitySectionId { get; private set; }
    public OfferActivitySection OfferActivitySection { get; private set; } = null!;
    public Guid SourceRequirementId { get; private set; }
    public Guid ProductTypeId { get; private set; }
    public string ProductTypeName { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; } = string.Empty;
    public decimal ConfiguredQuantity { get; private set; }
    public bool IsRequired { get; private set; }
    public ProductQuantityBehavior QuantityBehavior { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }
    public decimal CalculatedQuantity { get; private set; }
    public OfferProductLine? ProductLine { get; private set; }

    internal static OfferProductRequirement Create(
        OfferActivitySection section,
        ActivityProductRequirement sourceRequirement)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(sourceRequirement);

        var requirement = new OfferProductRequirement
        {
            Id = Guid.NewGuid(),
            OfferActivitySection = section,
            OfferActivitySectionId = section.Id,
            SourceRequirementId = sourceRequirement.Id,
            ProductTypeId = sourceRequirement.ProductTypeId,
            ProductTypeName = sourceRequirement.ProductType.Name,
            CategoryId = sourceRequirement.ProductType.CategoryId,
            CategoryName = sourceRequirement.ProductType.Category.Name,
            ConfiguredQuantity = sourceRequirement.Quantity,
            IsRequired = sourceRequirement.IsRequired,
            QuantityBehavior = sourceRequirement.QuantityBehavior,
            Notes = sourceRequirement.Notes,
            SortOrder = sourceRequirement.SortOrder
        };
        requirement.Recalculate(section.RequestedMeasurement, section.BasisQuantity);
        return requirement;
    }

    internal void Recalculate(decimal requestedMeasurement, decimal basisQuantity)
    {
        CalculatedQuantity = QuantityBehavior == ProductQuantityBehavior.Fixed
            ? OfferQuantity.RoundCalculated(ConfiguredQuantity)
            : OfferQuantity.CalculateProportional(
                ConfiguredQuantity,
                requestedMeasurement,
                basisQuantity);
        ProductLine?.SyncQuantity(this);
    }

    public OfferProductLine SelectProduct(Offer offer, Product product)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(product);
        if (ProductLine is { ProductId: var selectedProductId } existingLine
            && selectedProductId == product.Id)
        {
            return existingLine;
        }
        if (product.Status != ProductStatus.Active)
        {
            throw new InvalidOperationException("Only active products can satisfy an offer requirement.");
        }
        if (product.ProductTypeId != ProductTypeId)
        {
            throw new InvalidOperationException(
                "The selected product does not belong to the requirement product type.");
        }

        if (ProductLine is null)
        {
            ProductLine = OfferProductLine.Create(offer, this, product);
        }
        else
        {
            ProductLine.ReplaceProduct(this, product);
        }

        return ProductLine;
    }

    public OfferProductLine? ClearProduct()
    {
        var current = ProductLine;
        ProductLine = null;
        return current;
    }
}
