using Domain.Entities;

namespace Application.Assets;

public sealed record AssetCatalogItemListDto(
    Guid Id,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string ReviewStatus,
    DateTimeOffset Created,
    DateTimeOffset LastModified);

public sealed record AssetCatalogItemLookupDto(
    Guid Id,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model);

public sealed record AssetCatalogItemDto(
    Guid Id,
    string Classification,
    string Name,
    string? Manufacturer,
    string? Model,
    string? Description,
    string ReviewStatus,
    string? ReviewedById,
    string? ReviewedByDisplayName,
    DateTimeOffset? ReviewedAt,
    string? RejectionReason,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy,
    string ConcurrencyToken)
{
    public static AssetCatalogItemDto From(AssetCatalogItem item) => new(
        item.Id,
        item.Classification.ToString(),
        item.Name,
        item.Manufacturer,
        item.Model,
        item.Description,
        item.ReviewStatus.ToString(),
        item.ReviewedById,
        AssetMapping.DisplayName(item.ReviewedBy),
        item.ReviewedAt,
        item.RejectionReason,
        item.Created,
        item.CreatedBy,
        item.LastModified,
        item.LastModifiedBy,
        Convert.ToBase64String(item.RowVersion));
}

public sealed record AssetListDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string QrIdentifier,
    string? ManufacturerSerialNumber,
    Guid CatalogItemId,
    string CatalogName,
    string Classification,
    Guid SiteId,
    string SiteName,
    string? ResponsibleWorkerId,
    string? ResponsibleWorkerDisplayName,
    string Condition,
    string OperationalStatus,
    string ReviewStatus,
    bool HasPendingHandover);

public sealed record AssetDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string QrIdentifier,
    Guid CatalogItemId,
    string CatalogName,
    string Classification,
    string? Manufacturer,
    string? Model,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string SiteName,
    string? ResponsibleWorkerId,
    string? ResponsibleWorkerDisplayName,
    string Condition,
    string OperationalStatus,
    string ReviewStatus,
    string? PendingHandoverRecipientId,
    string? PendingHandoverRecipientDisplayName,
    string? PreHandoverOperationalStatus,
    long HistorySequence,
    DateTimeOffset Created,
    string? CreatedBy,
    DateTimeOffset LastModified,
    string? LastModifiedBy,
    string ConcurrencyToken)
{
    public static AssetDto From(Asset asset) => new(
        asset.Id,
        asset.NumberId,
        asset.AssetCode ?? string.Empty,
        asset.QrIdentifier.ToString(),
        asset.CatalogItemId,
        asset.CatalogItem.Name,
        asset.CatalogItem.Classification.ToString(),
        asset.CatalogItem.Manufacturer,
        asset.CatalogItem.Model,
        asset.ManufacturerSerialNumber,
        asset.SiteId,
        asset.Site.Name.Value,
        asset.ResponsibleWorkerId,
        AssetMapping.DisplayName(asset.ResponsibleWorker),
        asset.Condition.ToString(),
        asset.OperationalStatus.ToString(),
        asset.ReviewStatus.ToString(),
        asset.PendingHandoverRecipientId,
        AssetMapping.DisplayName(asset.PendingHandoverRecipient),
        asset.PreHandoverOperationalStatus?.ToString(),
        asset.HistorySequence,
        asset.Created,
        asset.CreatedBy,
        asset.LastModified,
        asset.LastModifiedBy,
        Convert.ToBase64String(asset.RowVersion));
}

public sealed record RegisteredAssetDto(
    Guid Id,
    int NumberId,
    string AssetCode,
    string QrIdentifier,
    string ConcurrencyToken);

public sealed record AssetEventSnapshotDto(
    Guid CatalogItemId,
    string? CatalogItemName,
    string QrIdentifier,
    string? ManufacturerSerialNumber,
    Guid SiteId,
    string? SiteName,
    string? ResponsibleWorkerId,
    string? ResponsibleWorkerDisplayName,
    string? PendingHandoverRecipientId,
    string? PendingHandoverRecipientDisplayName,
    string Condition,
    string OperationalStatus,
    string? PreHandoverOperationalStatus,
    string ReviewStatus);

public sealed record AssetEventDto(
    Guid Id,
    long SequenceNumber,
    string EventCode,
    string ActorId,
    string ActorDisplayName,
    DateTimeOffset OccurredAt,
    string? Note,
    AssetEventSnapshotDto? Before,
    AssetEventSnapshotDto After);

internal static class AssetMapping
{
    internal static string? DisplayName(ApplicationUser? user) =>
        user is null ? null : user.UserName ?? user.Email ?? user.Id;
}
