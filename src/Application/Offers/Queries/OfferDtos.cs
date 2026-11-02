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
    IReadOnlyList<OfferActivityDto> Activities)
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
                        section.PricingMode?.ToCode(),
                        section.PriceAmount,
                        TryCalculateSectionTotal(section),
                        section.SortOrder,
                        section.ProductRequirements
                            .OrderBy(requirement => requirement.SortOrder)
                            .ThenBy(requirement => requirement.Id)
                            .Select(ToRequirementDto)
                            .ToList()))
                    .ToList()))
            .ToList());

    public static OfferProductRequirementDto ToRequirementDto(
        OfferProductRequirement requirement) => new(
        requirement.Id,
        requirement.SourceRequirementId,
        requirement.ProductTypeId,
        requirement.ProductTypeName,
        requirement.CategoryId,
        requirement.CategoryName,
        requirement.ConfiguredQuantity,
        requirement.IsRequired,
        requirement.QuantityBehavior.ToCode(),
        requirement.Notes,
        requirement.SortOrder,
        requirement.CalculatedQuantity,
        requirement.ProductLine is null ? null : ToProductLineDto(requirement.ProductLine));

    public static OfferProductLineDto ToProductLineDto(OfferProductLine line) => new(
        line.Id,
        line.OfferProductRequirementId,
        line.ProductId,
        line.ProductNumberId,
        line.Title,
        line.ProductTypeId,
        line.ProductTypeName,
        line.CategoryId,
        line.CategoryName,
        line.Brand,
        line.Model,
        line.PackageQuantity,
        line.PackageUnit?.ToCode(),
        line.RequiredQuantity,
        line.OptionalQuantity);

    private static decimal? TryCalculateSectionTotal(OfferActivitySection section)
    {
        try
        {
            return section.CalculatePriceTotal();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or OverflowException)
        {
            return null;
        }
    }
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

/// <summary>Represents one snapshotted activity section and its Product Requirements.</summary>
public sealed record OfferActivitySectionDto(
    Guid Id,
    Guid SourceSectionId,
    string? Name,
    decimal BasisQuantity,
    string MeasurementUnit,
    decimal RequestedMeasurement,
    string? PricingMode,
    decimal? PriceAmount,
    decimal? PriceTotal,
    int SortOrder,
    IReadOnlyList<OfferProductRequirementDto> ProductRequirements);

/// <summary>Represents a snapshotted Product Requirement in an Offer.</summary>
public sealed record OfferProductRequirementDto(
    Guid Id,
    Guid SourceRequirementId,
    Guid ProductTypeId,
    string ProductTypeName,
    Guid CategoryId,
    string CategoryName,
    decimal ConfiguredQuantity,
    bool IsRequired,
    string QuantityBehavior,
    string? Notes,
    int SortOrder,
    decimal CalculatedQuantity,
    OfferProductLineDto? ProductLine);

/// <summary>Represents the concrete Product selected for one Offer Product Requirement.</summary>
public sealed record OfferProductLineDto(
    Guid Id,
    Guid OfferProductRequirementId,
    Guid ProductId,
    int ProductNumberId,
    string Title,
    Guid ProductTypeId,
    string ProductTypeName,
    Guid CategoryId,
    string CategoryName,
    string? Brand,
    string? Model,
    decimal? PackageQuantity,
    string? PackageUnit,
    decimal RequiredQuantity,
    decimal OptionalQuantity);
