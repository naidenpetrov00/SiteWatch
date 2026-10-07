using Domain.SeedWork;
using Domain.SeedWork.Enums;

namespace Domain.Entities;

public sealed class Proposal : BaseAuditableEntity, IHasNumberId, IAgregateRoot
{
    public const int MaxPublicNotesLength = 2000;
    public const int MaxPaymentTermsLength = 2000;
    public const string EuroCurrencyCode = "EUR";

    private readonly List<ProposalActivity> _activities = [];
    private readonly List<ProposalProductLine> _productLines = [];

    private Proposal()
    {
    }

    public int NumberId { get; private set; }
    public int RevisionNumber { get; private set; }
    public Guid SiteId { get; private set; }
    public Site Site { get; private set; } = null!;
    public Guid SourceOfferId { get; private set; }
    public Offer SourceOffer { get; private set; } = null!;
    public int SourceOfferNumberId { get; private set; }
    public DateTimeOffset SourceOfferFinalizedAt { get; private set; }
    public int SiteNumberId { get; private set; }
    public string SiteName { get; private set; } = string.Empty;
    public string SiteAddress { get; private set; } = string.Empty;
    public string RecipientUserId { get; private set; } = string.Empty;
    public string RecipientDisplayName { get; private set; } = string.Empty;
    public string RecipientEmail { get; private set; } = string.Empty;
    public string CurrencyCode { get; private set; } = EuroCurrencyCode;
    public ProposalStatus Status { get; private set; }
    public DateOnly? ValidUntil { get; private set; }
    public string? PublicNotes { get; private set; }
    public string? PaymentTerms { get; private set; }
    public decimal ActivitySubtotalBeforeDiscount { get; private set; }
    public decimal ActivityDiscountPercentage { get; private set; }
    public decimal ActivityDiscountAmount { get; private set; }
    public decimal ActivityTotalAfterDiscount { get; private set; }
    public decimal ProductSubtotalBeforeDiscount { get; private set; }
    public decimal ProductDiscountPercentage { get; private set; }
    public decimal ProductDiscountAmount { get; private set; }
    public decimal ProductTotalAfterDiscount { get; private set; }
    public decimal Total { get; private set; }
    public int UnpricedOptionalItemCount { get; private set; }
    public bool ExcludesUnpricedOptionalItems => UnpricedOptionalItemCount > 0;
    public DateTimeOffset? IssuedAt { get; private set; }
    public string? IssuedBy { get; private set; }
    public ProposalDocument? Document { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<ProposalActivity> Activities => _activities;
    public IReadOnlyCollection<ProposalProductLine> ProductLines => _productLines;

    public static Proposal Create(
        Offer sourceOffer,
        ApplicationUser recipient,
        int revisionNumber,
        int? proposalNumber = null)
    {
        ArgumentNullException.ThrowIfNull(sourceOffer);
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(revisionNumber);
        if (proposalNumber.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(proposalNumber.Value);
        }

        if (sourceOffer.Status != OfferStatus.Finalized
            || !sourceOffer.FinalizedAt.HasValue)
        {
            throw new InvalidOperationException(
                "A Proposal can only be created from a finalized Offer.");
        }

        if (sourceOffer.Activities.Count == 0)
        {
            throw new InvalidOperationException(
                "A Proposal requires at least one priced activity.");
        }

        ValidateDiscount(
            sourceOffer.ActivityDiscountPercentage,
            "activity discount");
        ValidateDiscount(
            sourceOffer.ProductDiscountPercentage,
            "product discount");

        var email = NormalizeRequired(recipient.Email, 256, "recipient email");
        var displayName = NormalizeRequired(
            recipient.UserName ?? recipient.Email ?? recipient.Id,
            450,
            "recipient display name");
        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            NumberId = proposalNumber ?? 0,
            RevisionNumber = revisionNumber,
            Site = sourceOffer.Site,
            SiteId = sourceOffer.SiteId,
            SourceOffer = sourceOffer,
            SourceOfferId = sourceOffer.Id,
            SourceOfferNumberId = sourceOffer.NumberId,
            SourceOfferFinalizedAt = sourceOffer.FinalizedAt.Value,
            SiteNumberId = sourceOffer.Site.NumberId,
            SiteName = sourceOffer.Site.Name.Value,
            SiteAddress = sourceOffer.Site.Address.Value,
            RecipientUserId = NormalizeRequired(recipient.Id, 450, "recipient user ID"),
            RecipientDisplayName = displayName,
            RecipientEmail = email,
            Status = ProposalStatus.Draft,
            ActivityDiscountPercentage = sourceOffer.ActivityDiscountPercentage,
            ProductDiscountPercentage = sourceOffer.ProductDiscountPercentage
        };

        foreach (var activity in sourceOffer.Activities
                     .OrderBy(item => item.SortOrder)
                     .ThenBy(item => item.Id))
        {
            proposal._activities.Add(ProposalActivity.Create(proposal, activity));
        }

        var productSortOrder = 0;
        foreach (var line in sourceOffer.ProductLines
                     .OrderBy(item => item.ProductNumberId)
                     .ThenBy(item => item.Id))
        {
            proposal._productLines.Add(
                ProposalProductLine.Create(proposal, line, productSortOrder++));
        }

        proposal.CalculateTotals();
        return proposal;
    }

    public void UpdateMetadata(
        DateOnly? validUntil,
        string? publicNotes,
        string? paymentTerms)
    {
        EnsureDraft();
        ValidUntil = validUntil;
        PublicNotes = NormalizeOptional(
            publicNotes,
            MaxPublicNotesLength,
            nameof(publicNotes));
        PaymentTerms = NormalizeOptional(
            paymentTerms,
            MaxPaymentTermsLength,
            nameof(paymentTerms));
    }

    public void EnsureCanIssue()
    {
        EnsureDraft();
        if (!ValidUntil.HasValue)
        {
            throw new InvalidOperationException(
                "Set a validity date before issuing the Proposal.");
        }

        if (string.IsNullOrWhiteSpace(RecipientUserId)
            || string.IsNullOrWhiteSpace(RecipientDisplayName)
            || string.IsNullOrWhiteSpace(RecipientEmail))
        {
            throw new InvalidOperationException(
                "The Proposal recipient snapshot is incomplete.");
        }

        if (_activities.Count == 0
            || _activities.Any(activity => activity.Sections.Count == 0)
            || _activities.SelectMany(activity => activity.Sections).Any(section =>
                section.CalculatedTotal < 0m
                || (section.PricingMode != ActivityPricingMode.Free
                    && (!section.PriceAmount.HasValue || section.PriceAmount.Value <= 0m)))
            || _productLines.Any(line =>
                (line.RequiredQuantity > 0m && !line.RequiredTotal.HasValue)
                || (line.OptionalQuantity > 0m
                    && line.SelectedPriceAmount.HasValue
                    && !line.OptionalTotal.HasValue)))
        {
            throw new InvalidOperationException(
                "The Proposal commercial snapshot is incomplete.");
        }
    }

    public void Issue(
        DateTimeOffset issuedAt,
        string issuedBy,
        ProposalDocument document)
    {
        EnsureCanIssue();
        ArgumentException.ThrowIfNullOrWhiteSpace(issuedBy);
        ArgumentNullException.ThrowIfNull(document);
        if (issuedAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Issue time must be UTC.", nameof(issuedAt));
        }

        if (document.ProposalId != Id)
        {
            throw new InvalidOperationException(
                "The Proposal document belongs to another Proposal.");
        }

        Document = document;
        Status = ProposalStatus.Issued;
        IssuedAt = issuedAt;
        IssuedBy = issuedBy;
    }

    private void CalculateTotals()
    {
        try
        {
            ActivitySubtotalBeforeDiscount = _activities
                .SelectMany(activity => activity.Sections)
                .Sum(section => section.CalculatedTotal);
            ProductSubtotalBeforeDiscount = _productLines.Sum(line =>
                (line.RequiredTotal ?? 0m) + (line.OptionalTotal ?? 0m));
            UnpricedOptionalItemCount = _productLines.Count(line =>
                line.IsOptionalPriceExcluded);
            ActivityDiscountAmount = CalculateDiscount(
                ActivitySubtotalBeforeDiscount,
                ActivityDiscountPercentage);
            ProductDiscountAmount = CalculateDiscount(
                ProductSubtotalBeforeDiscount,
                ProductDiscountPercentage);
            ActivityTotalAfterDiscount = ActivitySubtotalBeforeDiscount
                - ActivityDiscountAmount;
            ProductTotalAfterDiscount = ProductSubtotalBeforeDiscount
                - ProductDiscountAmount;
            Total = ActivityTotalAfterDiscount + ProductTotalAfterDiscount;
        }
        catch (OverflowException exception)
        {
            throw new InvalidOperationException(
                "The Proposal totals exceed the supported EUR range.",
                exception);
        }
    }

    private void EnsureDraft()
    {
        if (Status != ProposalStatus.Draft)
        {
            throw new InvalidOperationException("Issued Proposals are immutable.");
        }
    }

    private static decimal CalculateDiscount(decimal subtotal, decimal percentage) =>
        decimal.Round(
            subtotal * (percentage / 100m),
            2,
            MidpointRounding.AwayFromZero);

    private static void ValidateDiscount(decimal value, string description)
    {
        if (value is < 0m or > 100m || decimal.Round(value, 2) != value)
        {
            throw new InvalidOperationException(
                $"The Offer {description} is not valid for a Proposal.");
        }
    }

    private static string NormalizeRequired(
        string? value,
        int maxLength,
        string description)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"A Proposal requires a valid {description}.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException(
                $"The Proposal {description} is too long.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
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
