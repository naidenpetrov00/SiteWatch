using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class RetailerPriceCollectionRun : BaseAuditableEntity, IAgregateRoot
{
    private readonly List<RetailerPriceCollectionRunItem> _items = [];

    private RetailerPriceCollectionRun()
    {
    }

    public Guid CompanyPersonId { get; private set; }
    public Person CompanyPerson { get; private set; } = null!;
    public Guid? OfferId { get; private set; }
    public Offer? Offer { get; private set; }
    public Guid ExtractionProfileId { get; private set; }
    public RetailerExtractionProfile ExtractionProfile { get; private set; } = null!;
    public RetailerPriceCollectionRunStatus Status { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int TotalCount { get; private set; }
    public int QueuedCount { get; private set; }
    public int RunningCount { get; private set; }
    public int ProcessedCount { get; private set; }
    public int SucceededCount { get; private set; }
    public int FailedCount { get; private set; }
    public int SkippedCount { get; private set; }
    public IReadOnlyCollection<RetailerPriceCollectionRunItem> Items => _items;

    public static RetailerPriceCollectionRun Create(
        Person companyPerson,
        RetailerExtractionProfile extractionProfile,
        IEnumerable<RetailerListing> listings,
        DateTimeOffset requestedAt,
        string requestedBy,
        Offer? offer = null)
    {
        ArgumentNullException.ThrowIfNull(companyPerson);
        ArgumentNullException.ThrowIfNull(extractionProfile);
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedBy);
        if (companyPerson.Type != PersonType.Company)
        {
            throw new ArgumentException("A price-collection run must belong to a company Person.");
        }
        if (extractionProfile.Status != RetailerExtractionProfileStatus.Published
            || !extractionProfile.IsActive
            || extractionProfile.CompanyPersonId != companyPerson.Id)
        {
            throw new ArgumentException(
                "A price-collection run requires the company's active published profile.");
        }
        EnsureUtc(requestedAt, nameof(requestedAt));
        if (offer is not null && offer.Status != OfferStatus.Draft)
        {
            throw new ArgumentException(
                "An offer-scoped price-collection run requires a draft offer.",
                nameof(offer));
        }

        var run = new RetailerPriceCollectionRun
        {
            Id = Guid.NewGuid(),
            CompanyPerson = companyPerson,
            CompanyPersonId = companyPerson.Id,
            Offer = offer,
            OfferId = offer?.Id,
            ExtractionProfile = extractionProfile,
            ExtractionProfileId = extractionProfile.Id,
            Status = RetailerPriceCollectionRunStatus.Queued,
            RequestedAt = requestedAt,
            RequestedBy = requestedBy.Trim()
        };

        foreach (var listing in listings)
        {
            var item = RetailerPriceCollectionRunItem.Create(run, listing, requestedAt);
            run._items.Add(item);
            if (item.Status == RetailerPriceCollectionRunItemStatus.Queued)
            {
                run.QueuedCount++;
            }
            else
            {
                run.ProcessedCount++;
                run.SkippedCount++;
            }
        }

        run.TotalCount = run._items.Count;
        if (run.QueuedCount == 0)
        {
            run.Status = RetailerPriceCollectionRunStatus.Completed;
            run.CompletedAt = requestedAt;
        }
        run.EnsureCounterInvariants();
        return run;
    }

    public void EnsureCounterInvariants()
    {
        if (ProcessedCount != SucceededCount + FailedCount + SkippedCount
            || TotalCount != QueuedCount + RunningCount + ProcessedCount)
        {
            throw new InvalidOperationException("The price-collection run counters are inconsistent.");
        }
        if (Status == RetailerPriceCollectionRunStatus.CompletedWithFailures
            && FailedCount == 0)
        {
            throw new InvalidOperationException(
                "A run can be completed with failures only when at least one item failed.");
        }
        if (Status == RetailerPriceCollectionRunStatus.Completed && FailedCount != 0)
        {
            throw new InvalidOperationException("A completed run cannot contain failed items.");
        }
    }

    private static void EnsureUtc(DateTimeOffset value, string name)
    {
        if (value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Collection timestamps must be UTC.", name);
        }
    }
}
