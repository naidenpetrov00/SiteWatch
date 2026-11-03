using Domain.SeedWork;

namespace Domain.Entities;

public sealed class DefaultOfferRetailer : BaseEntity
{
    public const int MaximumCount = 3;

    private DefaultOfferRetailer()
    {
    }

    public Guid RetailerId { get; private set; }
    public Retailer Retailer { get; private set; } = null!;
    public int SortOrder { get; private set; }

    public static DefaultOfferRetailer Create(Retailer retailer, int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(retailer);
        if (sortOrder is < 0 or >= MaximumCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                $"Default retailer order must be from 0 through {MaximumCount - 1}.");
        }

        return new DefaultOfferRetailer
        {
            Id = Guid.NewGuid(),
            Retailer = retailer,
            RetailerId = retailer.Id,
            SortOrder = sortOrder
        };
    }
}
