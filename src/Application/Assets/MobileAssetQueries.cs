using System.Globalization;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Assets;

/// <summary>Looks up an active, accessible asset by its current QR identifier.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record MobileAssetByQrQuery : IRequest<MobileAssetDto>
{
    public string QrIdentifier { get; init; } = string.Empty;
}

/// <summary>Searches active assets accessible to the authenticated user.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record MobileAssetsQuery : IRequest<PagedResult<MobileAssetListDto>>
{
    public string Search { get; init; } = string.Empty;
    public bool AssignedToCurrentUser { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; } = 20;
}

/// <summary>Gets mobile-safe details for an active, accessible asset.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record MobileAssetByIdQuery(Guid Id) : IRequest<MobileAssetDto>;

/// <summary>Gets the authenticated user's own accessible registration.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record MobileAssetRegistrationByIdQuery(Guid Id)
    : IRequest<MobileAssetRegistrationDto>;

/// <summary>Searches Sites accessible to the authenticated user.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record MobileAssetSitesQuery : IRequest<IReadOnlyList<MobileAssetSiteDto>>
{
    public string? Search { get; init; }
}

public sealed class MobileAssetByQrHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<MobileAssetByQrQuery, MobileAssetDto>
{
    public async Task<MobileAssetDto> Handle(MobileAssetByQrQuery request, CancellationToken ct)
    {
        var userId = MobileAssetAccess.GetCurrentUserId(currentUser);
        var qrIdentifier = Guid.ParseExact(request.QrIdentifier, "D");
        var asset = await MobileAssetAccess.ActiveAssets(db, userId)
            .Include(x => x.Site)
            .Include(x => x.ResponsibleWorker)
            .SingleOrDefaultAsync(x => x.QrIdentifier == qrIdentifier, ct)
            ?? throw new NotFoundException(nameof(Asset), request.QrIdentifier);

        return MobileAssetDto.From(asset);
    }
}

public sealed class MobileAssetsHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<MobileAssetsQuery, PagedResult<MobileAssetListDto>>
{
    public async Task<PagedResult<MobileAssetListDto>> Handle(
        MobileAssetsQuery request,
        CancellationToken ct)
    {
        var userId = MobileAssetAccess.GetCurrentUserId(currentUser);
        var source = MobileAssetAccess.ActiveAssets(db, userId);
        if (request.AssignedToCurrentUser)
        {
            source = source.Where(x => x.ResponsibleWorkerId == userId);
        }

        var total = await source.CountAsync(ct);
        var search = request.Search.Trim();
        var assetCodeNumberId = 0;
        var hasAssetCode = search.StartsWith("AST-", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(
                search.AsSpan(4),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out assetCodeNumberId)
            && assetCodeNumberId > 0;
        var hasNumber = int.TryParse(
            search,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var numberId) && numberId > 0;
        var exactNumberId = hasAssetCode ? assetCodeNumberId : numberId;

        if (!string.IsNullOrWhiteSpace(search))
        {
            source = hasAssetCode
                ? source.Where(x => x.NumberId == exactNumberId)
                : source.Where(x =>
                    (hasNumber && x.NumberId == exactNumberId)
                    || x.Name.Contains(search)
                    || (x.Manufacturer != null && x.Manufacturer.Contains(search))
                    || (x.Model != null && x.Model.Contains(search))
                    || (x.ManufacturerSerialNumber != null
                        && x.ManufacturerSerialNumber.Contains(search)));
        }

        var filtered = await source.CountAsync(ct);
        var offset = (long)request.PageIndex * request.PageSize;
        var entities = offset > int.MaxValue
            ? new List<Asset>()
            : await source
            .OrderByDescending(x => (hasAssetCode || hasNumber)
                && x.NumberId == exactNumberId)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.NumberId)
            .Include(x => x.Site)
            .Skip((int)offset)
            .Take(request.PageSize)
            .ToListAsync(ct);
        var items = entities.Select(x => new MobileAssetListDto(
            x.Id,
            x.NumberId,
            x.AssetCode ?? string.Empty,
            x.Classification.ToString(),
            x.Name,
            x.Manufacturer,
            x.Model,
            x.ManufacturerSerialNumber,
            x.SiteId,
            x.Site.Name.Value,
            x.Condition.ToString(),
            x.OperationalStatus.ToString())).ToList();

        return new(items, filtered, total);
    }
}

public sealed class MobileAssetByIdHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<MobileAssetByIdQuery, MobileAssetDto>
{
    public async Task<MobileAssetDto> Handle(MobileAssetByIdQuery request, CancellationToken ct)
    {
        var userId = MobileAssetAccess.GetCurrentUserId(currentUser);
        var asset = await MobileAssetAccess.ActiveAssets(db, userId)
            .Include(x => x.Site)
            .Include(x => x.ResponsibleWorker)
            .SingleOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Asset), request.Id.ToString());

        return MobileAssetDto.From(asset);
    }
}

public sealed class MobileAssetRegistrationByIdHandler(
    IApplicationDbContext db,
    IUser currentUser)
    : IRequestHandler<MobileAssetRegistrationByIdQuery, MobileAssetRegistrationDto>
{
    public async Task<MobileAssetRegistrationDto> Handle(
        MobileAssetRegistrationByIdQuery request,
        CancellationToken ct)
    {
        var userId = MobileAssetAccess.GetCurrentUserId(currentUser);
        var asset = await db.Assets
            .AsNoTracking()
            .Where(x => x.CreatedBy == userId
                && x.Site.Users.Any(user => user.Id == userId))
            .Include(x => x.Site)
            .SingleOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Asset), request.Id.ToString());

        return MobileAssetRegistrationDto.From(asset, userId);
    }
}

public sealed class MobileAssetSitesHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<MobileAssetSitesQuery, IReadOnlyList<MobileAssetSiteDto>>
{
    public async Task<IReadOnlyList<MobileAssetSiteDto>> Handle(
        MobileAssetSitesQuery request,
        CancellationToken ct)
    {
        var userId = MobileAssetAccess.GetCurrentUserId(currentUser);
        var source = db.Sites
            .AsNoTracking()
            .Where(site => site.Users.Any(user => user.Id == userId));
        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            source = source.Where(site =>
                site.Name.Value.Contains(search) || site.Address.Value.Contains(search));
        }

        return await source
            .OrderBy(site => site.Name.Value)
            .ThenBy(site => site.NumberId)
            .Take(20)
            .Select(site => new MobileAssetSiteDto(
                site.Id,
                site.Name.Value,
                site.Address.Value,
                site.IsCompanyStock))
            .ToListAsync(ct);
    }
}

internal static class MobileAssetAccess
{
    internal static string GetCurrentUserId(IUser currentUser) =>
        string.IsNullOrWhiteSpace(currentUser.Id)
            ? throw new UnauthorizedAccessException()
            : currentUser.Id;

    internal static IQueryable<Asset> ActiveAssets(IApplicationDbContext db, string userId) =>
        db.Assets
            .AsNoTracking()
            .Where(asset =>
                asset.ReviewStatus == AssetReviewStatus.Active
                && asset.Site.Users.Any(user => user.Id == userId));
}
