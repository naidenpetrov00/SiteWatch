using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Offers.Queries;

/// <summary>Represents an offer in a site-scoped dashboard list.</summary>
public sealed record OfferSummaryDto(
    Guid Id,
    int NumberId,
    Guid SiteId,
    string? Title,
    string Status,
    DateTimeOffset Created,
    DateTimeOffset LastModified)
{
    public static OfferSummaryDto From(Offer offer) => new(
        offer.Id,
        offer.NumberId,
        offer.SiteId,
        offer.Title,
        offer.Status.ToString(),
        offer.Created,
        offer.LastModified);
}

/// <summary>Represents an offer and its associated site in the offer workspace.</summary>
public sealed record OfferDetailsDto(
    Guid Id,
    int NumberId,
    Guid SiteId,
    int SiteNumberId,
    string SiteName,
    string SiteAddress,
    string? Title,
    string? Notes,
    string Status,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy,
    DateTimeOffset? FinalizedAt,
    string? FinalizedBy,
    IReadOnlyList<OfferActivityDto> Activities,
    IReadOnlyList<OfferProductLineDto> Products)
{
    public static OfferDetailsDto From(Offer offer) => new(
        offer.Id,
        offer.NumberId,
        offer.SiteId,
        offer.Site.NumberId,
        offer.Site.Name.Value,
        offer.Site.Address.Value,
        offer.Title,
        offer.Notes,
        offer.Status.ToString(),
        offer.Created,
        offer.CreatedBy,
        offer.LastModified,
        offer.LastModifiedBy,
        offer.FinalizedAt,
        offer.FinalizedBy,
        offer.Activities
            .OrderBy(activity => activity.SortOrder)
            .ThenBy(activity => activity.Id)
            .Select(activity => new OfferActivityDto(
                activity.Id,
                activity.SourceActivityId,
                activity.ActivityNumberId,
                activity.Name,
                activity.Description,
                activity.SortOrder,
                activity.Sections
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Id)
                    .Select(section => new OfferActivitySectionDto(
                        section.Id,
                        section.SourceSectionId,
                        section.Name,
                        section.BasisQuantity,
                        section.MeasurementUnit.ToCode(),
                        section.RequestedMeasurement,
                        section.SortOrder))
                    .ToList()))
            .ToList(),
        offer.ProductLines
            .OrderBy(line => line.ProductNumberId)
            .ThenBy(line => line.Id)
            .Select(line => new OfferProductLineDto(
                line.Id,
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
                line.Contributions
                    .OrderBy(contribution => contribution.OfferActivitySection.OfferActivity.SortOrder)
                    .ThenBy(contribution => contribution.OfferActivitySection.SortOrder)
                    .ThenBy(contribution => contribution.SortOrder)
                    .ThenBy(contribution => contribution.Id)
                    .Select(contribution => new OfferProductContributionDto(
                        contribution.Id,
                        contribution.SourceRequirementId,
                        contribution.OfferActivitySection.OfferActivityId,
                        contribution.OfferActivitySection.OfferActivity.ActivityNumberId,
                        contribution.OfferActivitySection.OfferActivity.Name,
                        contribution.OfferActivitySectionId,
                        contribution.OfferActivitySection.SourceSectionId,
                        contribution.OfferActivitySection.Name,
                        contribution.OfferActivitySection.BasisQuantity,
                        contribution.OfferActivitySection.MeasurementUnit.ToCode(),
                        contribution.OfferActivitySection.RequestedMeasurement,
                        contribution.ConfiguredQuantity,
                        contribution.IsRequired,
                        contribution.QuantityBehavior.ToCode(),
                        contribution.Notes,
                        contribution.SortOrder,
                        contribution.CalculatedQuantity))
                    .ToList()))
            .ToList());
}

/// <summary>Represents one selected activity snapshot in an Offer.</summary>
public sealed record OfferActivityDto(
    Guid Id,
    Guid SourceActivityId,
    int ActivityNumberId,
    string Name,
    string? Description,
    int SortOrder,
    IReadOnlyList<OfferActivitySectionDto> Sections);

/// <summary>Represents one snapshotted activity section and its requested measurement.</summary>
public sealed record OfferActivitySectionDto(
    Guid Id,
    Guid SourceSectionId,
    string? Name,
    decimal BasisQuantity,
    string MeasurementUnit,
    decimal RequestedMeasurement,
    int SortOrder);

/// <summary>Represents one persisted aggregate Product line in an Offer.</summary>
public sealed record OfferProductLineDto(
    Guid Id,
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
    IReadOnlyList<OfferProductContributionDto> Contributions);

/// <summary>Explains one activity requirement's contribution to an aggregate Product line.</summary>
public sealed record OfferProductContributionDto(
    Guid Id,
    Guid SourceRequirementId,
    Guid OfferActivityId,
    int ActivityNumberId,
    string ActivityName,
    Guid OfferActivitySectionId,
    Guid SourceSectionId,
    string? SectionName,
    decimal BasisQuantity,
    string MeasurementUnit,
    decimal RequestedMeasurement,
    decimal ConfiguredQuantity,
    bool IsRequired,
    string QuantityBehavior,
    string? Notes,
    int SortOrder,
    decimal CalculatedQuantity);
