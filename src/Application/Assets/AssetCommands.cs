using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Assets;

public abstract record AssetDetailsRequest
{
    public string Classification { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Description { get; init; }
}

public sealed record AssetRegistrationRow(string? ManufacturerSerialNumber);

[Authorize(Policy = AuthorizationPolicies.Administrator)]
public sealed record RegisterAssetsCommand : AssetDetailsRequest,
    IRequest<IReadOnlyList<RegisteredAssetDto>>
{
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
public sealed record UpdateAssetDetailsCommand : AssetMutationRequest
{
    public string Classification { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Description { get; init; }
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

public sealed class RegisterAssetsHandler(
    IApplicationDbContext db,
    IUser currentUser,
    IIdentityService identity)
    : IRequestHandler<RegisterAssetsCommand, IReadOnlyList<RegisteredAssetDto>>
{
    public async Task<IReadOnlyList<RegisteredAssetDto>> Handle(
        RegisterAssetsCommand request,
        CancellationToken ct)
    {
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var site = await db.Sites.SingleOrDefaultAsync(x => x.Id == request.SiteId, ct)
            ?? throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        var worker = await AssetCommandSupport.GetWorkerAsync(
            db,
            identity,
            request.ResponsibleWorkerId,
            ct);
        var classification = AssetCommandSupport.Parse<AssetClassification>(
            request.Classification);
        var condition = AssetCommandSupport.Parse<AssetCondition>(request.Condition);
        var status = AssetCommandSupport.Parse<AssetOperationalStatus>(
            request.OperationalStatus);
        var now = DateTimeOffset.UtcNow;
        var assets = request.Rows
            .Select(row => AssetCommandSupport.CreateActiveAsset(
                classification,
                request.Name,
                request.Manufacturer,
                request.Model,
                request.Description,
                site,
                worker,
                row.ManufacturerSerialNumber,
                condition,
                status,
                actor,
                now,
                request.Note))
            .ToList();

        foreach (var asset in assets)
        {
            AssetCommandSupport.SetCreatedAudit(asset, actor.Id, now);
        }

        db.Assets.AddRange(assets);
        await AssetCommandSupport.SaveAsync(db, ct);
        return assets
            .Select(x => new RegisteredAssetDto(
                x.Id,
                x.NumberId,
                x.AssetCode ?? string.Empty,
                x.QrIdentifier.ToString(),
                Convert.ToBase64String(x.RowVersion)))
            .ToList();
    }
}

public sealed class ReviewAssetRegistrationHandler(
    IApplicationDbContext db,
    IUser currentUser)
    : IRequestHandler<ReviewAssetRegistrationCommand, AssetDto>
{
    public Task<AssetDto> Handle(ReviewAssetRegistrationCommand request, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) =>
            {
                if (request.Accepted)
                {
                    asset.ActivateRegistration(actor, now, request.Note);
                }
                else
                {
                    asset.RejectRegistration(actor, now, request.Note);
                }
            },
            ct);
}

public sealed class UpdateAssetDetailsHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<UpdateAssetDetailsCommand, AssetDto>
{
    public Task<AssetDto> Handle(UpdateAssetDetailsCommand request, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.UpdateDetails(
                AssetCommandSupport.Parse<AssetClassification>(request.Classification),
                request.Name,
                request.Manufacturer,
                request.Model,
                request.Description,
                request.ManufacturerSerialNumber,
                actor,
                now,
                request.Note),
            ct);
}

public sealed class RelocateAssetHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RelocateAssetCommand, AssetDto>
{
    public async Task<AssetDto> Handle(RelocateAssetCommand request, CancellationToken ct)
    {
        var site = await db.Sites.SingleOrDefaultAsync(x => x.Id == request.SiteId, ct)
            ?? throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        return await AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.Relocate(site, actor, now, request.Note),
            ct);
    }
}

public sealed class ChangeAssetConditionHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ChangeAssetConditionCommand, AssetDto>
{
    public Task<AssetDto> Handle(ChangeAssetConditionCommand request, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.ChangeCondition(
                AssetCommandSupport.Parse<AssetCondition>(request.Condition),
                actor,
                now,
                request.Note),
            ct);
}

public sealed class ChangeAssetOperationalStatusHandler(
    IApplicationDbContext db,
    IUser currentUser)
    : IRequestHandler<ChangeAssetOperationalStatusCommand, AssetDto>
{
    public Task<AssetDto> Handle(
        ChangeAssetOperationalStatusCommand request,
        CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.ChangeOperationalStatus(
                AssetCommandSupport.Parse<AssetOperationalStatus>(request.OperationalStatus),
                actor,
                now,
                request.Note),
            ct);
}

public sealed class ReportAssetMissingHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<ReportAssetMissingCommand, AssetDto>
{
    public Task<AssetDto> Handle(ReportAssetMissingCommand request, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.ReportMissing(actor, now, request.Note),
            ct);
}

public sealed class RetireAssetHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RetireAssetCommand, AssetDto>
{
    public Task<AssetDto> Handle(RetireAssetCommand request, CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.Retire(actor, now, request.Note),
            ct);
}

public sealed class ReassignAssetResponsibilityHandler(
    IApplicationDbContext db,
    IUser currentUser,
    IIdentityService identity)
    : IRequestHandler<ReassignAssetResponsibilityCommand, AssetDto>
{
    public async Task<AssetDto> Handle(
        ReassignAssetResponsibilityCommand request,
        CancellationToken ct)
    {
        var worker = await AssetCommandSupport.GetWorkerAsync(
            db,
            identity,
            request.ResponsibleWorkerId,
            ct);
        return await AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.AdministrativelyReassign(
                worker,
                actor,
                now,
                request.Reason),
            ct);
    }
}

public sealed class RotateAssetQrIdentifierHandler(
    IApplicationDbContext db,
    IUser currentUser)
    : IRequestHandler<RotateAssetQrIdentifierCommand, AssetDto>
{
    public Task<AssetDto> Handle(
        RotateAssetQrIdentifierCommand request,
        CancellationToken ct) =>
        AssetCommandSupport.MutateAsync(
            db,
            currentUser,
            request,
            (asset, actor, now) => asset.RotateQrIdentifier(actor, now, request.Note),
            ct);
}

internal static class AssetCommandSupport
{
    internal static T Parse<T>(string value) where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new ArgumentException($"Unsupported {typeof(T).Name}.");

    internal static async Task<ApplicationUser> GetActorAsync(
        IApplicationDbContext db,
        IUser user,
        CancellationToken ct)
    {
        var id = user.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new UnauthorizedAccessException();
        }

        return await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new UnauthorizedAccessException();
    }

    internal static async Task<ApplicationUser?> GetWorkerAsync(
        IApplicationDbContext db,
        IIdentityService identity,
        string? id,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var worker = await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException(nameof(ApplicationUser), id);
        if (!await identity.IsInRoleAsync(id, UserRoles.Worker))
        {
            throw new AssetInvalidTransitionException("The selected user is not a Worker.");
        }

        return worker;
    }

    internal static Asset CreateActiveAsset(
        AssetClassification classification,
        string name,
        string? manufacturer,
        string? model,
        string? description,
        Site site,
        ApplicationUser? worker,
        string? serial,
        AssetCondition condition,
        AssetOperationalStatus status,
        ApplicationUser actor,
        DateTimeOffset now,
        string? note)
    {
        try
        {
            return Asset.RegisterActive(
                classification,
                name,
                manufacturer,
                model,
                description,
                site,
                worker,
                serial,
                condition,
                status,
                actor,
                now,
                note);
        }
        catch (InvalidOperationException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
    }

    internal static async Task<AssetDto> MutateAsync(
        IApplicationDbContext db,
        IUser currentUser,
        AssetMutationRequest request,
        Action<Asset, ApplicationUser, DateTimeOffset> operation,
        CancellationToken ct)
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
        try
        {
            action();
        }
        catch (InvalidOperationException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
    }

    internal static T RunTransition<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (InvalidOperationException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            throw new AssetInvalidTransitionException(ex.Message, ex);
        }
    }

    internal static void EnsureConcurrency(byte[] current, string token)
    {
        byte[] supplied;
        try
        {
            supplied = Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            throw new AssetConcurrencyConflictException("The concurrency token is invalid.");
        }

        if (!current.AsSpan().SequenceEqual(supplied))
        {
            throw new AssetConcurrencyConflictException(
                "The resource changed after it was loaded.");
        }
    }

    internal static void SetCreatedAudit(
        Domain.SeedWork.BaseAuditableEntity entity,
        string actorId,
        DateTimeOffset now)
    {
        entity.Created = now;
        entity.CreatedBy = actorId;
        entity.LastModified = now;
        entity.LastModifiedBy = actorId;
    }

    internal static void SetModifiedAudit(
        Domain.SeedWork.BaseAuditableEntity entity,
        string actorId,
        DateTimeOffset now)
    {
        entity.LastModified = now;
        entity.LastModifiedBy = actorId;
    }

    internal static async Task SaveAsync(IApplicationDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AssetConcurrencyConflictException(
                "The resource changed while the operation was being saved.");
        }
    }
}
