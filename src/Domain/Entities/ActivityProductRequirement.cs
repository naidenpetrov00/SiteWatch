using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class ActivityProductRequirement : BaseAuditableEntity
{
    public const int MaxNotesLength = 1000;

    private ActivityProductRequirement()
    {
    }

    public Guid SectionId { get; private set; }
    public ActivityRequirementSection Section { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public bool IsRequired { get; private set; }
    public ProductQuantityBehavior QuantityBehavior { get; private set; }
    public string? Notes { get; private set; }
    public int SortOrder { get; private set; }

    internal static ActivityProductRequirement Create(
        ActivityRequirementSection section,
        Product product,
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes,
        int sortOrder)
    {
        if (product.Status != ProductStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active products can be assigned to a requirement section.");
        }

        var requirement = new ActivityProductRequirement
        {
            Id = Guid.NewGuid(),
            Section = section,
            SectionId = section.Id,
            Product = product,
            ProductId = product.Id
        };
        requirement.UpdateDetails(quantity, isRequired, quantityBehavior, notes);
        requirement.SetSortOrder(sortOrder);
        return requirement;
    }

    public void UpdateDetails(
        decimal quantity,
        bool isRequired,
        ProductQuantityBehavior quantityBehavior,
        string? notes)
    {
        if (quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "Quantity must be greater than zero.");
        }

        if (decimal.Round(quantity, 4) != quantity)
        {
            throw new ArgumentException(
                "Quantity cannot have more than four decimal places.",
                nameof(quantity));
        }

        if (!Enum.IsDefined(quantityBehavior))
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantityBehavior),
                quantityBehavior,
                "Unsupported product quantity behavior.");
        }

        Quantity = quantity;
        IsRequired = isRequired;
        QuantityBehavior = quantityBehavior;
        Notes = NormalizeNotes(notes);
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

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalizedNotes = notes.Trim();
        return normalizedNotes.Length <= MaxNotesLength
            ? normalizedNotes
            : throw new ArgumentOutOfRangeException(
                nameof(notes),
                normalizedNotes.Length,
                $"Product requirement notes cannot exceed {MaxNotesLength} characters.");
    }
}
