using Domain.Entities;

namespace Application.Assets;

/// <summary>Represents an asset in mobile search results.</summary>
public sealed record MobileAssetListDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string SiteName,
    string Condition,
    string OperationalStatus);

/// <summary>Represents mobile-safe details for an active asset.</summary>
public sealed record MobileAssetDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string QrIdentifier,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string? Description,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string SiteName,
    string? ResponsibleAssigneeDisplayName,
    string Condition,
    string OperationalStatus)
{
    internal static MobileAssetDto From(Asset asset) => new(
        asset.Id,
        asset.NumberId,
        asset.AssetCode ?? string.Empty,
        asset.QrIdentifier.ToString("D"),
        asset.Classification.ToString(),
        asset.Name,
        asset.Manufacturer,
        asset.Model,
        asset.Description,
        asset.ManufacturerSerialNumber,
        asset.SiteId,
        asset.Site.Name.Value,
        AssetMapping.DisplayName(asset.ResponsibleWorker),
        asset.Condition.ToString(),
        asset.OperationalStatus.ToString());
}

/// <summary>Represents a user's own asset registration without review audit details.</summary>
public sealed record MobileAssetRegistrationDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string QrIdentifier,
    string ReviewStatus,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string? Description,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string SiteName,
    bool IsAssignedToCurrentUser,
    string Condition,
    string OperationalStatus)
{
    internal static MobileAssetRegistrationDto From(Asset asset, string currentUserId) => new(
        asset.Id,
        asset.NumberId,
        asset.AssetCode ?? string.Empty,
        asset.QrIdentifier.ToString("D"),
        asset.ReviewStatus.ToString(),
        asset.Classification.ToString(),
        asset.Name,
        asset.Manufacturer,
        asset.Model,
        asset.Description,
        asset.ManufacturerSerialNumber,
        asset.SiteId,
        asset.Site.Name.Value,
        asset.ResponsibleWorkerId == currentUserId,
        asset.Condition.ToString(),
        asset.OperationalStatus.ToString());
}

/// <summary>Represents a Site available to the authenticated user.</summary>
public sealed record MobileAssetSiteDto(
    Guid Id,
    string Name,
    string Address,
    bool IsCompanyStock);
