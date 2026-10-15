using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Proposals;

/// <summary>Represents one Proposal revision in source-Offer history.</summary>
public sealed record ProposalSummaryDto(
    Guid Id,
    int NumberId,
    int RevisionNumber,
    Guid SiteId,
    Guid SourceOfferId,
    int SourceOfferNumberId,
    string RecipientDisplayName,
    string RecipientEmail,
    string Status,
    DateOnly? ValidUntil,
    DateTimeOffset Created,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? FirstViewedAt,
    DateTimeOffset? RespondedAt,
    string? ResponseComment,
    decimal Total,
    string CurrencyCode,
    bool ExcludesUnpricedOptionalItems,
    int UnpricedOptionalItemCount,
    bool CanRespond,
    int? SupersededByRevisionNumber)
{
    public static ProposalSummaryDto From(
        Proposal proposal,
        int? supersededByRevisionNumber = null) => new(
        proposal.Id,
        proposal.NumberId,
        proposal.RevisionNumber,
        proposal.SiteId,
        proposal.SourceOfferId,
        proposal.SourceOfferNumberId,
        proposal.RecipientDisplayName,
        proposal.RecipientEmail,
        proposal.Status.ToString(),
        proposal.ValidUntil,
        proposal.Created,
        proposal.IssuedAt,
        proposal.FirstViewedAt,
        proposal.RespondedAt,
        proposal.ResponseComment,
        proposal.Total,
        proposal.CurrencyCode,
        proposal.ExcludesUnpricedOptionalItems,
        proposal.UnpricedOptionalItemCount,
        proposal.Status == ProposalStatus.Issued
            && !supersededByRevisionNumber.HasValue,
        supersededByRevisionNumber);
}

/// <summary>Represents a complete immutable commercial Proposal snapshot.</summary>
public sealed record ProposalDetailsDto(
    Guid Id,
    int NumberId,
    int RevisionNumber,
    Guid SiteId,
    int SiteNumberId,
    string SiteName,
    string SiteAddress,
    Guid SourceOfferId,
    int SourceOfferNumberId,
    DateTimeOffset SourceOfferFinalizedAt,
    string RecipientUserId,
    string RecipientDisplayName,
    string RecipientEmail,
    string Status,
    DateOnly? ValidUntil,
    string? PublicNotes,
    string? PaymentTerms,
    DateTimeOffset Created,
    DateTimeOffset LastModified,
    DateTimeOffset? IssuedAt,
    string? IssuedBy,
    DateTimeOffset? FirstViewedAt,
    DateTimeOffset? RespondedAt,
    string? RespondedByUserId,
    string? ResponseComment,
    bool CanRespond,
    int? SupersededByRevisionNumber,
    string CurrencyCode,
    decimal ActivitySubtotalBeforeDiscount,
    decimal ActivityDiscountPercentage,
    decimal ActivityDiscountAmount,
    decimal ActivityTotalAfterDiscount,
    decimal ProductSubtotalBeforeDiscount,
    decimal ProductDiscountPercentage,
    decimal ProductDiscountAmount,
    decimal ProductTotalAfterDiscount,
    decimal Total,
    bool ExcludesUnpricedOptionalItems,
    int UnpricedOptionalItemCount,
    bool HasPdf,
    IReadOnlyList<ProposalActivityDto> Activities,
    IReadOnlyList<ProposalProductLineDto> Products)
{
    public static ProposalDetailsDto From(
        Proposal proposal,
        int? supersededByRevisionNumber = null) => new(
        proposal.Id,
        proposal.NumberId,
        proposal.RevisionNumber,
        proposal.SiteId,
        proposal.SiteNumberId,
        proposal.SiteName,
        proposal.SiteAddress,
        proposal.SourceOfferId,
        proposal.SourceOfferNumberId,
        proposal.SourceOfferFinalizedAt,
        proposal.RecipientUserId,
        proposal.RecipientDisplayName,
        proposal.RecipientEmail,
        proposal.Status.ToString(),
        proposal.ValidUntil,
        proposal.PublicNotes,
        proposal.PaymentTerms,
        proposal.Created,
        proposal.LastModified,
        proposal.IssuedAt,
        proposal.IssuedBy,
        proposal.FirstViewedAt,
        proposal.RespondedAt,
        proposal.RespondedByUserId,
        proposal.ResponseComment,
        proposal.Status == ProposalStatus.Issued
            && !supersededByRevisionNumber.HasValue,
        supersededByRevisionNumber,
        proposal.CurrencyCode,
        proposal.ActivitySubtotalBeforeDiscount,
        proposal.ActivityDiscountPercentage,
        proposal.ActivityDiscountAmount,
        proposal.ActivityTotalAfterDiscount,
        proposal.ProductSubtotalBeforeDiscount,
        proposal.ProductDiscountPercentage,
        proposal.ProductDiscountAmount,
        proposal.ProductTotalAfterDiscount,
        proposal.Total,
        proposal.ExcludesUnpricedOptionalItems,
        proposal.UnpricedOptionalItemCount,
        proposal.Document is not null,
        proposal.Activities
            .OrderBy(activity => activity.SortOrder)
            .ThenBy(activity => activity.Id)
            .Select(activity => new ProposalActivityDto(
                activity.Id,
                activity.SourceOfferActivityId,
                activity.ActivityNumberId,
                activity.Name,
                activity.Description,
                activity.SortOrder,
                activity.Sections
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Id)
                    .Select(section => new ProposalActivitySectionDto(
                        section.Id,
                        section.SourceOfferActivitySectionId,
                        section.Name,
                        section.BasisQuantity,
                        section.MeasurementUnit.ToCode(),
                        section.RequestedMeasurement,
                        section.PricingMode.ToCode(),
                        section.PriceAmount,
                        section.CalculatedTotal,
                        section.SortOrder))
                    .ToList()))
            .ToList(),
        proposal.ProductLines
            .OrderBy(line => line.SortOrder)
            .ThenBy(line => line.Id)
            .Select(line => new ProposalProductLineDto(
                line.Id,
                line.SourceOfferProductLineId,
                line.ProductId,
                line.ProductNumberId,
                line.Title,
                line.Category.ToCode(),
                line.Brand,
                line.Model,
                line.PackageQuantity,
                line.PackageUnit?.ToCode(),
                line.RequiredQuantity,
                line.OptionalQuantity,
                line.SelectedRetailerId,
                line.SelectedRetailerDisplayName,
                line.SelectedPriceAmount,
                line.SelectedPriceCurrencyCode,
                line.SelectedPriceBasis?.ToCode(),
                line.RequiredTotal,
                line.OptionalTotal,
                line.IsOptionalPriceExcluded,
                line.SortOrder))
            .ToList());
}

/// <summary>Represents one client-visible Proposal revision.</summary>
public sealed record ClientProposalSummaryDto(
    Guid Id,
    int NumberId,
    int RevisionNumber,
    string Status,
    DateOnly? ValidUntil,
    DateTimeOffset IssuedAt,
    DateTimeOffset? FirstViewedAt,
    DateTimeOffset? RespondedAt,
    string? ResponseComment,
    decimal Total,
    string CurrencyCode,
    string SiteName,
    bool ExcludesUnpricedOptionalItems,
    bool CanRespond,
    int? SupersededByRevisionNumber)
{
    public static ClientProposalSummaryDto From(
        Proposal proposal,
        int? supersededByRevisionNumber) => new(
        proposal.Id,
        proposal.NumberId,
        proposal.RevisionNumber,
        proposal.Status.ToString(),
        proposal.ValidUntil,
        proposal.IssuedAt!.Value,
        proposal.FirstViewedAt,
        proposal.RespondedAt,
        proposal.ResponseComment,
        proposal.Total,
        proposal.CurrencyCode,
        proposal.SiteName,
        proposal.ExcludesUnpricedOptionalItems,
        proposal.Status == ProposalStatus.Issued
            && !supersededByRevisionNumber.HasValue,
        supersededByRevisionNumber);
}

/// <summary>Represents an immutable Proposal snapshot visible to its recipient.</summary>
public sealed record ClientProposalDetailsDto(
    Guid Id,
    int NumberId,
    int RevisionNumber,
    string Status,
    DateOnly? ValidUntil,
    DateTimeOffset IssuedAt,
    DateTimeOffset? FirstViewedAt,
    DateTimeOffset? RespondedAt,
    string? ResponseComment,
    bool CanRespond,
    int? SupersededByRevisionNumber,
    int SiteNumberId,
    string SiteName,
    string SiteAddress,
    string RecipientDisplayName,
    string RecipientEmail,
    string? PublicNotes,
    string? PaymentTerms,
    string CurrencyCode,
    decimal ActivitySubtotalBeforeDiscount,
    decimal ActivityDiscountPercentage,
    decimal ActivityDiscountAmount,
    decimal ActivityTotalAfterDiscount,
    decimal ProductSubtotalBeforeDiscount,
    decimal ProductDiscountPercentage,
    decimal ProductDiscountAmount,
    decimal ProductTotalAfterDiscount,
    decimal Total,
    bool ExcludesUnpricedOptionalItems,
    int UnpricedOptionalItemCount,
    bool HasPdf,
    IReadOnlyList<ProposalActivityDto> Activities,
    IReadOnlyList<ProposalProductLineDto> Products)
{
    public static ClientProposalDetailsDto From(
        Proposal proposal,
        int? supersededByRevisionNumber)
    {
        var details = ProposalDetailsDto.From(
            proposal,
            supersededByRevisionNumber);
        return new ClientProposalDetailsDto(
            details.Id,
            details.NumberId,
            details.RevisionNumber,
            details.Status,
            details.ValidUntil,
            details.IssuedAt!.Value,
            details.FirstViewedAt,
            details.RespondedAt,
            details.ResponseComment,
            details.CanRespond,
            details.SupersededByRevisionNumber,
            details.SiteNumberId,
            details.SiteName,
            details.SiteAddress,
            details.RecipientDisplayName,
            details.RecipientEmail,
            details.PublicNotes,
            details.PaymentTerms,
            details.CurrencyCode,
            details.ActivitySubtotalBeforeDiscount,
            details.ActivityDiscountPercentage,
            details.ActivityDiscountAmount,
            details.ActivityTotalAfterDiscount,
            details.ProductSubtotalBeforeDiscount,
            details.ProductDiscountPercentage,
            details.ProductDiscountAmount,
            details.ProductTotalAfterDiscount,
            details.Total,
            details.ExcludesUnpricedOptionalItems,
            details.UnpricedOptionalItemCount,
            details.HasPdf,
            details.Activities,
            details.Products);
    }
}

/// <summary>Represents a snapshotted Proposal activity.</summary>
public sealed record ProposalActivityDto(
    Guid Id,
    Guid SourceOfferActivityId,
    int ActivityNumberId,
    string Name,
    string? Description,
    int SortOrder,
    IReadOnlyList<ProposalActivitySectionDto> Sections);

/// <summary>Represents one priced activity section in a Proposal snapshot.</summary>
public sealed record ProposalActivitySectionDto(
    Guid Id,
    Guid SourceOfferActivitySectionId,
    string? Name,
    decimal BasisQuantity,
    string MeasurementUnit,
    decimal RequestedMeasurement,
    string PricingMode,
    decimal? PriceAmount,
    decimal CalculatedTotal,
    int SortOrder);

/// <summary>Represents one Product line in a Proposal snapshot.</summary>
public sealed record ProposalProductLineDto(
    Guid Id,
    Guid SourceOfferProductLineId,
    Guid ProductId,
    int ProductNumberId,
    string Title,
    string Category,
    string? Brand,
    string? Model,
    decimal? PackageQuantity,
    string? PackageUnit,
    decimal RequiredQuantity,
    decimal OptionalQuantity,
    Guid? SelectedRetailerId,
    string? SelectedRetailerDisplayName,
    decimal? SelectedPriceAmount,
    string? SelectedPriceCurrencyCode,
    string? SelectedPriceBasis,
    decimal? RequiredTotal,
    decimal? OptionalTotal,
    bool IsOptionalPriceExcluded,
    int SortOrder);

/// <summary>Describes the stored PDF attached to an issued Proposal.</summary>
public sealed record ProposalPdfInfoDto(string FileName, string ContentType);
