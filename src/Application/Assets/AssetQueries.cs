using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Assets;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AssetCatalogItemsQuery : TableQueryRequest, IRequest<PagedResult<AssetCatalogItemListDto>>
{
    public string? Search { get; init; }
    public string? Classification { get; init; }
    public string? ReviewStatus { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record AssetCatalogItemSearchQuery : IRequest<List<AssetCatalogItemLookupDto>>
{
    public string? SearchTerm { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record AssetCatalogItemByIdQuery(Guid Id) : IRequest<AssetCatalogItemDto>;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed class AssetsQuery : TableQueryRequest, IRequest<PagedResult<AssetListDto>>
{
    public string? Search { get; init; }
    public string? AssetCode { get; init; }
    public int? NumberId { get; init; }
    public Guid? QrIdentifier { get; init; }
    public string? ManufacturerSerialNumber { get; init; }
    public string? CatalogName { get; init; }
    public string? Classification { get; init; }
    public Guid? CatalogItemId { get; init; }
    public string? SiteName { get; init; }
    public Guid? SiteId { get; init; }
    public string? ResponsibleWorker { get; init; }
    public string? ResponsibleWorkerId { get; init; }
    public string? Condition { get; init; }
    public string? OperationalStatus { get; init; }
    public string? ReviewStatus { get; init; }
    public bool? HasPendingHandover { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record AssetByIdQuery(Guid Id) : IRequest<AssetDto>;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record AssetEventsQuery(Guid Id) : IRequest<PagedResult<AssetEventDto>>
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 50;
}

public sealed class AssetQueriesValidator :
    AbstractValidator<AssetsQuery>
{
    public AssetQueriesValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.AssetCode).MaximumLength(32);
        RuleFor(x => x.ManufacturerSerialNumber).MaximumLength(Asset.MaxManufacturerSerialNumberLength);
        RuleFor(x => x.CatalogName).MaximumLength(AssetCatalogItem.MaxNameLength);
        RuleFor(x => x.SiteName).MaximumLength(200);
        RuleFor(x => x.ResponsibleWorker).MaximumLength(200);
        RuleFor(x => x.ResponsibleWorkerId).MaximumLength(450);
        RuleFor(x => x.Classification).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetCatalogClassification>(x));
        RuleFor(x => x.Condition).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetCondition>(x));
        RuleFor(x => x.OperationalStatus).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetOperationalStatus>(x));
        RuleFor(x => x.ReviewStatus).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetReviewStatus>(x));
    }
}

public sealed class AssetCatalogItemsQueryValidator : AbstractValidator<AssetCatalogItemsQuery>
{
    public AssetCatalogItemsQueryValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.Classification).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetCatalogClassification>(x));
        RuleFor(x => x.ReviewStatus).Must(x => AssetValidationExtensions.IsOptionalEnum<AssetReviewStatus>(x));
    }
}

public sealed class AssetCatalogItemSearchQueryValidator : AbstractValidator<AssetCatalogItemSearchQuery>
{
    public AssetCatalogItemSearchQueryValidator() =>
        RuleFor(x => x.SearchTerm).MaximumLength(200);
}

public sealed class AssetEventsQueryValidator : AbstractValidator<AssetEventsQuery>
{
    public AssetEventsQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class AssetCatalogItemsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetCatalogItemsQuery, PagedResult<AssetCatalogItemListDto>>
{
    public async Task<PagedResult<AssetCatalogItemListDto>> Handle(AssetCatalogItemsQuery request, CancellationToken ct)
    {
        var source = dbContext.AssetCatalogItems.AsNoTracking();
        var total = await source.CountAsync(ct);
        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            source = source.Where(x => x.Name.Contains(search)
                || (x.Manufacturer != null && x.Manufacturer.Contains(search))
                || (x.Model != null && x.Model.Contains(search)));
        }
        if (Enum.TryParse<AssetCatalogClassification>(request.Classification, true, out var classification))
            source = source.Where(x => x.Classification == classification);
        if (Enum.TryParse<AssetReviewStatus>(request.ReviewStatus, true, out var reviewStatus))
            source = source.Where(x => x.ReviewStatus == reviewStatus);

        var filtered = await source.CountAsync(ct);
        source = request.SortActive?.ToLowerInvariant() switch
        {
            "classification" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.Classification).ThenBy(x => x.Name) : source.OrderBy(x => x.Classification).ThenBy(x => x.Name),
            "reviewstatus" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.ReviewStatus).ThenBy(x => x.Name) : source.OrderBy(x => x.ReviewStatus).ThenBy(x => x.Name),
            "manufacturer" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.Manufacturer).ThenBy(x => x.Name) : source.OrderBy(x => x.Manufacturer).ThenBy(x => x.Name),
            _ => request.SortDirection == "desc" ? source.OrderByDescending(x => x.Name).ThenBy(x => x.Id) : source.OrderBy(x => x.Name).ThenBy(x => x.Id)
        };

        var entities = await source.Skip(request.PageIndex * request.PageSize).Take(request.PageSize)
            .ToListAsync(ct);
        var items = entities.Select(x => new AssetCatalogItemListDto(
            x.Id,
            x.Classification.ToString(),
            x.Name,
            x.Manufacturer,
            x.Model,
            x.ReviewStatus.ToString(),
            x.Created,
            x.LastModified)).ToList();
        return new(items, filtered, total);
    }
}

public sealed class AssetCatalogItemSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetCatalogItemSearchQuery, List<AssetCatalogItemLookupDto>>
{
    public async Task<List<AssetCatalogItemLookupDto>> Handle(AssetCatalogItemSearchQuery request, CancellationToken ct)
    {
        var term = request.SearchTerm?.Trim();
        if (string.IsNullOrWhiteSpace(term)) return [];
        var items = await dbContext.AssetCatalogItems.AsNoTracking()
            .Where(x => x.ReviewStatus == AssetReviewStatus.Active && (x.Name.Contains(term) || (x.Manufacturer != null && x.Manufacturer.Contains(term)) || (x.Model != null && x.Model.Contains(term))))
            .OrderBy(x => x.Name).ThenBy(x => x.Id).Take(20)
            .ToListAsync(ct);
        return items.Select(x => new AssetCatalogItemLookupDto(
            x.Id,
            x.Classification.ToString(),
            x.Name,
            x.Manufacturer,
            x.Model)).ToList();
    }
}

public sealed class AssetCatalogItemByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetCatalogItemByIdQuery, AssetCatalogItemDto>
{
    public async Task<AssetCatalogItemDto> Handle(AssetCatalogItemByIdQuery request, CancellationToken ct)
    {
        var item = await dbContext.AssetCatalogItems.AsNoTracking().Include(x => x.ReviewedBy)
            .SingleOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(AssetCatalogItem), request.Id.ToString());
        return AssetCatalogItemDto.From(item);
    }
}

public sealed class AssetsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetsQuery, PagedResult<AssetListDto>>
{
    public async Task<PagedResult<AssetListDto>> Handle(AssetsQuery request, CancellationToken ct)
    {
        var source = dbContext.Assets.AsNoTracking();
        var total = await source.CountAsync(ct);
        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var codeText = search.StartsWith("AST-", StringComparison.OrdinalIgnoreCase) ? search[4..] : search;
            var hasNumber = int.TryParse(codeText, out var number);
            var hasQr = Guid.TryParse(search, out var qr);
            source = source.Where(x => (hasNumber && x.NumberId == number) || (hasQr && x.QrIdentifier == qr)
                || (x.ManufacturerSerialNumber != null && x.ManufacturerSerialNumber.Contains(search))
                || x.CatalogItem.Name.Contains(search) || x.Site.Name.Value.Contains(search)
                || (x.ResponsibleWorker != null && ((x.ResponsibleWorker.UserName != null && x.ResponsibleWorker.UserName.Contains(search)) || (x.ResponsibleWorker.Email != null && x.ResponsibleWorker.Email.Contains(search)))));
        }
        if (!string.IsNullOrWhiteSpace(request.AssetCode))
        {
            var code = request.AssetCode.Trim();
            if (code.StartsWith("AST-", StringComparison.OrdinalIgnoreCase)) code = code[4..];
            source = int.TryParse(code, out var number) ? source.Where(x => x.NumberId == number) : source.Where(_ => false);
        }
        if (request.NumberId.HasValue) source = source.Where(x => x.NumberId == request.NumberId);
        if (request.QrIdentifier.HasValue) source = source.Where(x => x.QrIdentifier == request.QrIdentifier);
        if (!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumber)) source = source.Where(x => x.ManufacturerSerialNumber != null && x.ManufacturerSerialNumber.Contains(request.ManufacturerSerialNumber));
        if (!string.IsNullOrWhiteSpace(request.CatalogName)) source = source.Where(x => x.CatalogItem.Name.Contains(request.CatalogName));
        if (request.CatalogItemId.HasValue) source = source.Where(x => x.CatalogItemId == request.CatalogItemId);
        if (!string.IsNullOrWhiteSpace(request.SiteName)) source = source.Where(x => x.Site.Name.Value.Contains(request.SiteName));
        if (request.SiteId.HasValue) source = source.Where(x => x.SiteId == request.SiteId);
        if (!string.IsNullOrWhiteSpace(request.ResponsibleWorker)) source = source.Where(x => x.ResponsibleWorker != null && ((x.ResponsibleWorker.UserName != null && x.ResponsibleWorker.UserName.Contains(request.ResponsibleWorker)) || (x.ResponsibleWorker.Email != null && x.ResponsibleWorker.Email.Contains(request.ResponsibleWorker))));
        if (!string.IsNullOrWhiteSpace(request.ResponsibleWorkerId)) source = source.Where(x => x.ResponsibleWorkerId == request.ResponsibleWorkerId);
        if (Enum.TryParse<AssetCatalogClassification>(request.Classification, true, out var classification)) source = source.Where(x => x.CatalogItem.Classification == classification);
        if (Enum.TryParse<AssetCondition>(request.Condition, true, out var condition)) source = source.Where(x => x.Condition == condition);
        if (Enum.TryParse<AssetOperationalStatus>(request.OperationalStatus, true, out var status)) source = source.Where(x => x.OperationalStatus == status);
        if (Enum.TryParse<AssetReviewStatus>(request.ReviewStatus, true, out var review)) source = source.Where(x => x.ReviewStatus == review);
        if (request.HasPendingHandover.HasValue) source = source.Where(x => (x.PendingHandoverRecipientId != null) == request.HasPendingHandover.Value);
        var filtered = await source.CountAsync(ct);

        source = request.SortActive?.ToLowerInvariant() switch
        {
            "catalogname" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.CatalogItem.Name).ThenBy(x => x.NumberId) : source.OrderBy(x => x.CatalogItem.Name).ThenBy(x => x.NumberId),
            "classification" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.CatalogItem.Classification).ThenBy(x => x.NumberId) : source.OrderBy(x => x.CatalogItem.Classification).ThenBy(x => x.NumberId),
            "sitename" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.Site.Name.Value).ThenBy(x => x.NumberId) : source.OrderBy(x => x.Site.Name.Value).ThenBy(x => x.NumberId),
            "manufacturerserialnumber" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.ManufacturerSerialNumber).ThenBy(x => x.NumberId) : source.OrderBy(x => x.ManufacturerSerialNumber).ThenBy(x => x.NumberId),
            "responsibleworkerdisplayname" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.ResponsibleWorker!.UserName).ThenBy(x => x.NumberId) : source.OrderBy(x => x.ResponsibleWorker!.UserName).ThenBy(x => x.NumberId),
            "condition" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.Condition).ThenBy(x => x.NumberId) : source.OrderBy(x => x.Condition).ThenBy(x => x.NumberId),
            "operationalstatus" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.OperationalStatus).ThenBy(x => x.NumberId) : source.OrderBy(x => x.OperationalStatus).ThenBy(x => x.NumberId),
            "reviewstatus" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.ReviewStatus).ThenBy(x => x.NumberId) : source.OrderBy(x => x.ReviewStatus).ThenBy(x => x.NumberId),
            "haspendinghandover" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.PendingHandoverRecipientId != null).ThenBy(x => x.NumberId) : source.OrderBy(x => x.PendingHandoverRecipientId != null).ThenBy(x => x.NumberId),
            "qridentifier" => request.SortDirection == "desc" ? source.OrderByDescending(x => x.QrIdentifier).ThenBy(x => x.NumberId) : source.OrderBy(x => x.QrIdentifier).ThenBy(x => x.NumberId),
            _ => request.SortDirection == "desc" ? source.OrderByDescending(x => x.NumberId) : source.OrderBy(x => x.NumberId)
        };

        var entities = await source.Include(x => x.CatalogItem).Include(x => x.Site).Include(x => x.ResponsibleWorker)
            .Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        var items = entities.Select(x => new AssetListDto(x.Id, x.NumberId, x.AssetCode ?? string.Empty, x.QrIdentifier.ToString(), x.ManufacturerSerialNumber, x.CatalogItemId, x.CatalogItem.Name, x.CatalogItem.Classification.ToString(), x.SiteId, x.Site.Name.Value, x.ResponsibleWorkerId, AssetMapping.DisplayName(x.ResponsibleWorker), x.Condition.ToString(), x.OperationalStatus.ToString(), x.ReviewStatus.ToString(), x.PendingHandoverRecipientId != null)).ToList();
        return new(items, filtered, total);
    }
}

public sealed class AssetByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetByIdQuery, AssetDto>
{
    public async Task<AssetDto> Handle(AssetByIdQuery request, CancellationToken ct) =>
        AssetDto.From(await AssetQuerySupport.LoadAssetAsync(dbContext, request.Id, false, ct));
}

public sealed class AssetEventsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AssetEventsQuery, PagedResult<AssetEventDto>>
{
    public async Task<PagedResult<AssetEventDto>> Handle(AssetEventsQuery request, CancellationToken ct)
    {
        if (!await dbContext.Assets.AsNoTracking().AnyAsync(x => x.Id == request.Id, ct))
            throw new NotFoundException(nameof(Asset), request.Id.ToString());
        var source = dbContext.AssetEvents.AsNoTracking().Where(x => x.AssetId == request.Id);
        var total = await source.CountAsync(ct);
        var events = await source.Include(x => x.Actor).OrderBy(x => x.SequenceNumber)
            .Skip(request.PageIndex * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        var catalogIds = events.SelectMany(x => new Guid?[] { x.PreviousCatalogItemId, x.ResultingCatalogItemId }).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var siteIds = events.SelectMany(x => new Guid?[] { x.PreviousSiteId, x.ResultingSiteId }).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var userIds = events.SelectMany(x => new[] { x.PreviousResponsibleWorkerId, x.ResultingResponsibleWorkerId, x.PreviousPendingHandoverRecipientId, x.ResultingPendingHandoverRecipientId }).Where(x => x != null).Distinct().ToList();
        var catalogs = await dbContext.AssetCatalogItems.AsNoTracking().Where(x => catalogIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var sites = await dbContext.Sites.AsNoTracking().Where(x => siteIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name.Value, ct);
        var users = await dbContext.Users.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? x.Id, ct);
        return new(events.Select(x => MapEvent(x, catalogs, sites, users)).ToList(), total, total);
    }

    private static AssetEventDto MapEvent(AssetEvent e, IReadOnlyDictionary<Guid, string> catalogs, IReadOnlyDictionary<Guid, string> sites, IReadOnlyDictionary<string, string> users) => new(
        e.Id, e.SequenceNumber, e.EventType.ToString(), e.ActorId, AssetMapping.DisplayName(e.Actor) ?? e.ActorId, e.OccurredAt, e.Note,
        e.PreviousCatalogItemId.HasValue ? Snapshot(e.PreviousCatalogItemId.Value, e.PreviousQrIdentifier!.Value, e.PreviousManufacturerSerialNumber, e.PreviousSiteId!.Value, e.PreviousResponsibleWorkerId, e.PreviousPendingHandoverRecipientId, e.PreviousCondition!.Value, e.PreviousOperationalStatus!.Value, e.PreviousPreHandoverOperationalStatus, e.PreviousReviewStatus!.Value, catalogs, sites, users) : null,
        Snapshot(e.ResultingCatalogItemId, e.ResultingQrIdentifier, e.ResultingManufacturerSerialNumber, e.ResultingSiteId, e.ResultingResponsibleWorkerId, e.ResultingPendingHandoverRecipientId, e.ResultingCondition, e.ResultingOperationalStatus, e.ResultingPreHandoverOperationalStatus, e.ResultingReviewStatus, catalogs, sites, users));

    private static AssetEventSnapshotDto Snapshot(Guid catalogId, Guid qr, string? serial, Guid siteId, string? workerId, string? recipientId, AssetCondition condition, AssetOperationalStatus status, AssetOperationalStatus? preStatus, AssetReviewStatus review, IReadOnlyDictionary<Guid, string> catalogs, IReadOnlyDictionary<Guid, string> sites, IReadOnlyDictionary<string, string> users) =>
        new(catalogId, catalogs.GetValueOrDefault(catalogId), qr.ToString(), serial, siteId, sites.GetValueOrDefault(siteId), workerId, workerId is null ? null : users.GetValueOrDefault(workerId), recipientId, recipientId is null ? null : users.GetValueOrDefault(recipientId), condition.ToString(), status.ToString(), preStatus?.ToString(), review.ToString());
}

internal static class AssetQuerySupport
{
    internal static async Task<Asset> LoadAssetAsync(IApplicationDbContext dbContext, Guid id, bool tracked, CancellationToken ct)
    {
        IQueryable<Asset> query = dbContext.Assets.Include(x => x.CatalogItem).Include(x => x.Site).Include(x => x.ResponsibleWorker).Include(x => x.PendingHandoverRecipient);
        if (!tracked) query = query.AsNoTracking();
        return await query.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Asset), id.ToString());
    }
}

internal static class AssetValidationExtensions
{
    internal static bool IsOptionalEnum<TEnum>(string? value) where TEnum : struct, Enum =>
        string.IsNullOrWhiteSpace(value) || (Enum.TryParse<TEnum>(value, true, out var result) && Enum.IsDefined(result));
}
