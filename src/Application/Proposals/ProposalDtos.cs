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
    decimal Total,
    string CurrencyCode,
    bool ExcludesUnpricedOptionalItems,
    int UnpricedOptionalItemCount)
{
    public static ProposalSummaryDto From(Proposal proposal) => new(
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
        proposal.Total,
        proposal.CurrencyCode,
        proposal.ExcludesUnpricedOptionalItems,
        proposal.UnpricedOptionalItemCount);
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
    public static ProposalDetailsDto From(Proposal proposal) => new(
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
