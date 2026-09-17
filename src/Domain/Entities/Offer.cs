using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Offer : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 2000;

    private Offer()
    {
    }

    public int NumberId { get; private set; }
    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public string? Title { get; private set; }
    public string? Notes { get; private set; }
    public OfferStatus Status { get; private set; }

    public static Offer Create(Site site)
    {
        var normalizedSite = Guard.Against.Null(site);

        return new Offer
        {
            Id = Guid.NewGuid(),
            SiteId = normalizedSite.Id,
            Site = normalizedSite,
            Status = OfferStatus.Draft
        };
    }

    public void UpdateMetadata(string? title, string? notes)
    {
        EnsureDraft();
        Title = NormalizeOptional(title, MaxTitleLength, nameof(title));
        Notes = NormalizeOptional(notes, MaxNotesLength, nameof(notes), collapseWhitespace: false);
    }

    public bool Archive()
    {
        if (Status == OfferStatus.Archived)
        {
            return false;
        }

        Status = OfferStatus.Archived;
        return true;
    }

    private void EnsureDraft()
    {
        if (Status != OfferStatus.Draft)
        {
            throw new InvalidOperationException("Only draft offers can be edited.");
        }
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName,
        bool collapseWhitespace = true)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = collapseWhitespace
            ? string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            : value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                normalized.Length,
                $"The value must be at most {maxLength} characters.");
        }

        return normalized;
    }
}
