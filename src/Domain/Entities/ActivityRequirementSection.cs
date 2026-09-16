using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class ActivityRequirementSection : BaseAuditableEntity
{
    public const int MaxNameLength = 200;

    private readonly List<ActivityProductRequirement> _productRequirements = [];

    private ActivityRequirementSection()
    {
    }

    public Guid ActivityId { get; private set; }
    public Activity Activity { get; private set; } = null!;
    public string? Name { get; private set; }
    /// <summary>
    /// Measurement represented by the configured product quantities. Future calculations
    /// use requested measurement divided by this value as the section multiplier.
    /// </summary>
    public decimal BasisQuantity { get; private set; }
    public ActivityMeasurementUnit MeasurementUnit { get; private set; }
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<ActivityProductRequirement> ProductRequirements =>
        _productRequirements;

    internal static ActivityRequirementSection Create(
        Activity activity,
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit,
        int sortOrder)
    {
        var section = new ActivityRequirementSection
        {
            Id = Guid.NewGuid(),
            Activity = activity,
            ActivityId = activity.Id
        };
        section.UpdateDetails(name, basisQuantity, measurementUnit);
        section.SetSortOrder(sortOrder);
        return section;
    }

    public void UpdateDetails(
        string? name,
        decimal basisQuantity,
        ActivityMeasurementUnit measurementUnit)
    {
        Name = NormalizeOptionalName(name);
        BasisQuantity = ValidatePositiveQuantity(basisQuantity, nameof(basisQuantity));
        MeasurementUnit = Enum.IsDefined(measurementUnit)
            ? measurementUnit
            : throw new ArgumentOutOfRangeException(
                nameof(measurementUnit),
                measurementUnit,
                "Unsupported activity measurement unit.");
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                "Sort order cannot be negative.");
        }

        SortOrder = sortOrder;
    }

    public ActivityProductRequirement AddProductRequirement(
        Product product,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        int sortOrder)
    {
        if (_productRequirements.Any(requirement => requirement.ProductId == product.Id))
        {
            throw new InvalidOperationException(
                "The product is already assigned to this requirement section.");
        }

        var requirement = ActivityProductRequirement.Create(
            this,
            product,
            quantity,
            isRequired,
            quantityBehavior,
            notes,
            sortOrder);
        _productRequirements.Add(requirement);
        return requirement;
    }

    public void RemoveProductRequirement(ActivityProductRequirement requirement)
    {
        if (requirement.SectionId != Id || !_productRequirements.Remove(requirement))
        {
            throw new InvalidOperationException(
                "The product requirement does not belong to this section.");
        }
    }

    private static string? NormalizeOptionalName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var normalizedName = string.Join(
            " ",
            name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalizedName.Length <= MaxNameLength
            ? normalizedName
            : throw new ArgumentOutOfRangeException(
                nameof(name),
                normalizedName.Length,
                $"A requirement section name cannot exceed {MaxNameLength} characters.");
    }

    private static decimal ValidatePositiveQuantity(decimal value, string parameterName)
    {
        if (value <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Quantity must be greater than zero.");
        }

        if (decimal.Round(value, 4) != value)
        {
            throw new ArgumentException(
                "Quantity cannot have more than four decimal places.",
                parameterName);
        }

        return value;
    }
}
