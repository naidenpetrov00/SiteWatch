using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Assets;

public abstract record AssetCatalogDetailsRequest
{
    public string Classification { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Description { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record CreateAssetCatalogItemCommand : AssetCatalogDetailsRequest, IRequest<AssetCatalogItemDto>;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record UpdateAssetCatalogItemCommand : AssetCatalogDetailsRequest, IRequest<AssetCatalogItemDto>
{
    public Guid Id { get; init; }
    public string ConcurrencyToken { get; init; } = string.Empty;
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ReviewAssetCatalogItemCommand : IRequest<AssetCatalogItemDto>
{
    public Guid Id { get; init; }
    public bool Accepted { get; init; }
    public string? RejectionReason { get; init; }
    public string ConcurrencyToken { get; init; } = string.Empty;
}

public sealed record AssetRegistrationRow(string? ManufacturerSerialNumber);

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record RegisterAssetsCommand : IRequest<IReadOnlyList<RegisteredAssetDto>>
{
    public Guid CatalogItemId { get; init; }
    public Guid SiteId { get; init; }
    public string? ResponsibleWorkerId { get; init; }
    public string Condition { get; init; } = string.Empty;
    public string OperationalStatus { get; init; } = string.Empty;
    public string? Note { get; init; }
    public IReadOnlyList<AssetRegistrationRow> Rows { get; init; } = [];
}

public abstract record AssetMutationRequest : IRequest<AssetDto>
{
    public Guid Id { get; init; }
    public string ConcurrencyToken { get; init; } = string.Empty;
    public string? Note { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ReviewAssetRegistrationCommand : AssetMutationRequest
{
    public bool Accepted { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record CorrectAssetCatalogCommand : AssetMutationRequest
{
    public Guid CatalogItemId { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record CorrectAssetSerialCommand : AssetMutationRequest
{
    public string? ManufacturerSerialNumber { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record RelocateAssetCommand : AssetMutationRequest
{
    public Guid SiteId { get; init; }
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ChangeAssetConditionCommand : AssetMutationRequest
{
    public string Condition { get; init; } = string.Empty;
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ChangeAssetOperationalStatusCommand : AssetMutationRequest
{
    public string OperationalStatus { get; init; } = string.Empty;
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ReportAssetMissingCommand : AssetMutationRequest;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record RetireAssetCommand : AssetMutationRequest;

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record ReassignAssetResponsibilityCommand : AssetMutationRequest
{
    public string? ResponsibleWorkerId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record RotateAssetQrIdentifierCommand : AssetMutationRequest;

public sealed class CreateAssetCatalogItemHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<CreateAssetCatalogItemCommand, AssetCatalogItemDto>
{
    public async Task<AssetCatalogItemDto> Handle(CreateAssetCatalogItemCommand request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var item = AssetCatalogItem.CreateActive(AssetCommandSupport.Parse<AssetCatalogClassification>(request.Classification), request.Name, request.Manufacturer, request.Model, request.Description, actor, now);
        AssetCommandSupport.SetCreatedAudit(item, actor.Id, now);
        db.AssetCatalogItems.Add(item);
        await AssetCommandSupport.SaveAsync(db, ct);
        return AssetCatalogItemDto.From(item);
    }
}

public sealed class UpdateAssetCatalogItemHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<UpdateAssetCatalogItemCommand, AssetCatalogItemDto>
{
    public async Task<AssetCatalogItemDto> Handle(UpdateAssetCatalogItemCommand request, CancellationToken ct)
    {
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var item = await AssetCommandSupport.GetCatalogAsync(db, request.Id, ct);
        AssetCommandSupport.EnsureConcurrency(item.RowVersion, request.ConcurrencyToken);
        var now = DateTimeOffset.UtcNow;
        AssetCommandSupport.RunTransition(() => item.UpdateDetails(AssetCommandSupport.Parse<AssetCatalogClassification>(request.Classification), request.Name, request.Manufacturer, request.Model, request.Description));
        AssetCommandSupport.SetModifiedAudit(item, actor.Id, now);
        await AssetCommandSupport.SaveAsync(db, ct);
        return AssetCatalogItemDto.From(item);
    }
}

public sealed class ReviewAssetCatalogItemHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ReviewAssetCatalogItemCommand, AssetCatalogItemDto>
{
    public async Task<AssetCatalogItemDto> Handle(ReviewAssetCatalogItemCommand request, CancellationToken ct)
    {
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var item = await AssetCommandSupport.GetCatalogAsync(db, request.Id, ct);
        AssetCommandSupport.EnsureConcurrency(item.RowVersion, request.ConcurrencyToken);
        var now = DateTimeOffset.UtcNow;
        AssetCommandSupport.RunTransition(() => { if (request.Accepted) item.Activate(actor, now); else item.Reject(actor, now, request.RejectionReason); });
        AssetCommandSupport.SetModifiedAudit(item, actor.Id, now);
        await AssetCommandSupport.SaveAsync(db, ct);
        return AssetCatalogItemDto.From(item);
    }
}

public sealed class RegisterAssetsHandler(IApplicationDbContext db, IUser currentUser, IIdentityService identity)
    : IRequestHandler<RegisterAssetsCommand, IReadOnlyList<RegisteredAssetDto>>
{
    public async Task<IReadOnlyList<RegisteredAssetDto>> Handle(RegisterAssetsCommand request, CancellationToken ct)
    {
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var catalog = await AssetCommandSupport.GetCatalogAsync(db, request.CatalogItemId, ct);
        var site = await db.Sites.SingleOrDefaultAsync(x => x.Id == request.SiteId, ct) ?? throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        var worker = await AssetCommandSupport.GetWorkerAsync(db, identity, request.ResponsibleWorkerId, ct);
        var condition = AssetCommandSupport.Parse<AssetCondition>(request.Condition);
        var status = AssetCommandSupport.Parse<AssetOperationalStatus>(request.OperationalStatus);
        var now = DateTimeOffset.UtcNow;
        var assets = request.Rows.Select(row => AssetCommandSupport.CreateActiveAsset(catalog, site, worker, row.ManufacturerSerialNumber, condition, status, actor, now, request.Note)).ToList();
        foreach (var asset in assets) AssetCommandSupport.SetCreatedAudit(asset, actor.Id, now);
        db.Assets.AddRange(assets);
        await AssetCommandSupport.SaveAsync(db, ct);
        return assets.Select(x => new RegisteredAssetDto(x.Id, x.NumberId, x.AssetCode ?? string.Empty, x.QrIdentifier.ToString(), Convert.ToBase64String(x.RowVersion))).ToList();
    }
}

public sealed class ReviewAssetRegistrationHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ReviewAssetRegistrationCommand, AssetDto>
{
    public Task<AssetDto> Handle(ReviewAssetRegistrationCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => { if (r.Accepted) a.ActivateRegistration(actor, now, r.Note); else a.RejectRegistration(actor, now, r.Note); }, ct);
}

public sealed class CorrectAssetCatalogHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<CorrectAssetCatalogCommand, AssetDto>
{
    public async Task<AssetDto> Handle(CorrectAssetCatalogCommand r, CancellationToken ct)
    {
        var catalog = await AssetCommandSupport.GetCatalogAsync(db, r.CatalogItemId, ct);
        return await AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.CorrectCatalogAssociation(catalog, actor, now, r.Note), ct);
    }
}

public sealed class CorrectAssetSerialHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<CorrectAssetSerialCommand, AssetDto>
{
    public Task<AssetDto> Handle(CorrectAssetSerialCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.CorrectManufacturerSerialNumber(r.ManufacturerSerialNumber, actor, now, r.Note), ct);
}

public sealed class RelocateAssetHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RelocateAssetCommand, AssetDto>
{
    public async Task<AssetDto> Handle(RelocateAssetCommand r, CancellationToken ct)
    {
        var site = await db.Sites.SingleOrDefaultAsync(x => x.Id == r.SiteId, ct) ?? throw new NotFoundException(nameof(Site), r.SiteId.ToString());
        return await AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.Relocate(site, actor, now, r.Note), ct);
    }
}

public sealed class ChangeAssetConditionHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ChangeAssetConditionCommand, AssetDto>
{
    public Task<AssetDto> Handle(ChangeAssetConditionCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.ChangeCondition(AssetCommandSupport.Parse<AssetCondition>(r.Condition), actor, now, r.Note), ct);
}

public sealed class ChangeAssetOperationalStatusHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ChangeAssetOperationalStatusCommand, AssetDto>
{
    public Task<AssetDto> Handle(ChangeAssetOperationalStatusCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.ChangeOperationalStatus(AssetCommandSupport.Parse<AssetOperationalStatus>(r.OperationalStatus), actor, now, r.Note), ct);
}

public sealed class ReportAssetMissingHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ReportAssetMissingCommand, AssetDto>
{
    public Task<AssetDto> Handle(ReportAssetMissingCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.ReportMissing(actor, now, r.Note), ct);
}

public sealed class RetireAssetHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RetireAssetCommand, AssetDto>
{
    public Task<AssetDto> Handle(RetireAssetCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.Retire(actor, now, r.Note), ct);
}

public sealed class ReassignAssetResponsibilityHandler(IApplicationDbContext db, IUser currentUser, IIdentityService identity)
    : IRequestHandler<ReassignAssetResponsibilityCommand, AssetDto>
{
    public async Task<AssetDto> Handle(ReassignAssetResponsibilityCommand r, CancellationToken ct)
    {
        var worker = await AssetCommandSupport.GetWorkerAsync(db, identity, r.ResponsibleWorkerId, ct);
        return await AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.AdministrativelyReassign(worker, actor, now, r.Reason), ct);
    }
}

public sealed class RotateAssetQrIdentifierHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RotateAssetQrIdentifierCommand, AssetDto>
{
    public Task<AssetDto> Handle(RotateAssetQrIdentifierCommand r, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(db, currentUser, r, (a, actor, now) => a.RotateQrIdentifier(actor, now, r.Note), ct);
}

internal static class AssetCommandSupport
{
    internal static T Parse<T>(string value) where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed) ? parsed : throw new ArgumentException($"Unsupported {typeof(T).Name}.");

    internal static async Task<ApplicationUser> GetActorAsync(IApplicationDbContext db, IUser user, CancellationToken ct)
    {
        var id = user.Id;
        if (string.IsNullOrWhiteSpace(id)) throw new UnauthorizedAccessException();
        return await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new UnauthorizedAccessException();
    }

    internal static async Task<ApplicationUser?> GetWorkerAsync(IApplicationDbContext db, IIdentityService identity, string? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var worker = await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException(nameof(ApplicationUser), id);
        if (!await identity.IsInRoleAsync(id, UserRoles.Worker)) throw new AssetInvalidTransitionException("The selected user is not a Worker.");
        return worker;
    }

    internal static async Task<AssetCatalogItem> GetCatalogAsync(IApplicationDbContext db, Guid id, CancellationToken ct) =>
        await db.AssetCatalogItems.Include(x => x.ReviewedBy).SingleOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException(nameof(AssetCatalogItem), id.ToString());

    internal static Asset CreateActiveAsset(AssetCatalogItem catalog, Site site, ApplicationUser? worker, string? serial, AssetCondition condition, AssetOperationalStatus status, ApplicationUser actor, DateTimeOffset now, string? note)
    {
        try { return Asset.RegisterActive(catalog, site, worker, serial, condition, status, actor, now, note); }
        catch (InvalidOperationException ex) { throw new AssetInvalidTransitionException(ex.Message, ex); }
        catch (ArgumentException ex) { throw new AssetInvalidTransitionException(ex.Message, ex); }
    }

    internal static async Task<AssetDto> MutateAsync(IApplicationDbContext db, IUser currentUser, AssetMutationRequest request, Action<Asset, ApplicationUser, DateTimeOffset> operation, CancellationToken ct)
    {
        var actor = await GetActorAsync(db, currentUser, ct);
        var asset = await AssetQuerySupport.LoadAssetAsync(db, request.Id, true, ct);
        EnsureConcurrency(asset.RowVersion, request.ConcurrencyToken);
        var now = DateTimeOffset.UtcNow;
        RunTransition(() => operation(asset, actor, now));
        SetModifiedAudit(asset, actor.Id, now);
        await SaveAsync(db, ct);
        return AssetDto.From(asset);
    }

    internal static void RunTransition(Action action)
    {
        try { action(); }
        catch (InvalidOperationException ex) { throw new AssetInvalidTransitionException(ex.Message, ex); }
        catch (ArgumentException ex) { throw new AssetInvalidTransitionException(ex.Message, ex); }
    }

    internal static void EnsureConcurrency(byte[] current, string token)
    {
        byte[] supplied;
        try { supplied = Convert.FromBase64String(token); }
        catch (FormatException) { throw new AssetConcurrencyConflictException("The concurrency token is invalid."); }
        if (!current.AsSpan().SequenceEqual(supplied)) throw new AssetConcurrencyConflictException("The resource changed after it was loaded.");
    }

    internal static void SetCreatedAudit(Domain.SeedWork.BaseAuditableEntity entity, string actorId, DateTimeOffset now)
    {
        entity.Created = now; entity.CreatedBy = actorId; entity.LastModified = now; entity.LastModifiedBy = actorId;
    }

    internal static void SetModifiedAudit(Domain.SeedWork.BaseAuditableEntity entity, string actorId, DateTimeOffset now)
    {
        entity.LastModified = now; entity.LastModifiedBy = actorId;
    }

    internal static async Task SaveAsync(IApplicationDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AssetConcurrencyConflictException("The resource changed while the operation was being saved."); }
    }
}
