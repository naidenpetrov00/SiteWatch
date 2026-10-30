using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Assets;

/// <summary>Registers one mobile-submitted asset for administrator review.</summary>
[Authorize(Policy = AuthorizationPolicies.AdministratorOrWorker)]
public sealed record RegisterMobileAssetCommand : IRequest<MobileAssetRegistrationDto>
{
    public string Classification { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? Description { get; init; }
    public string? ManufacturerSerialNumber { get; init; }
    public Guid SiteId { get; init; }
    public bool AssignToMe { get; init; }
}

public sealed class RegisterMobileAssetHandler(IApplicationDbContext db, IUser currentUser)
    : IRequestHandler<RegisterMobileAssetCommand, MobileAssetRegistrationDto>
{
    public async Task<MobileAssetRegistrationDto> Handle(
        RegisterMobileAssetCommand request,
        CancellationToken ct)
    {
        var actor = await AssetCommandSupport.GetActorAsync(db, currentUser, ct);
        var site = await db.Sites
            .SingleOrDefaultAsync(
                candidate => candidate.Id == request.SiteId
                    && candidate.Users.Any(user => user.Id == actor.Id),
                ct)
            ?? throw new NotFoundException(nameof(Site), request.SiteId.ToString());
        var now = DateTimeOffset.UtcNow;
        var asset = AssetCommandSupport.RunTransition(() => Asset.RegisterPendingReview(
            AssetCommandSupport.Parse<AssetClassification>(request.Classification),
            request.Name,
            request.Manufacturer,
            request.Model,
            request.Description,
            site,
            request.AssignToMe ? actor : null,
            request.ManufacturerSerialNumber,
            AssetCondition.Good,
            AssetOperationalStatus.Available,
            actor,
            now));

        AssetCommandSupport.SetCreatedAudit(asset, actor.Id, now);
        db.Assets.Add(asset);
        await AssetCommandSupport.SaveAsync(db, ct);
        return MobileAssetRegistrationDto.From(asset, actor.Id);
    }
}
