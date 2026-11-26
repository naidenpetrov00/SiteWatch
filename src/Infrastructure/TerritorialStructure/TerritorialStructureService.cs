using Application.SeedWork.Interfaces;
using Application.TerritorialStructure;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.SeedWork;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.TerritorialStructure;

public sealed class TerritorialStructureService(ApplicationDbContext dbContext, IUser user)
    : ITerritorialStructureService
{
    public async Task<Guid> CreateAsync(TerritorialEntryKind kind, Guid? parentId, string name, CancellationToken cancellationToken)
    {
        BaseAuditableEntity entry = kind switch
        {
            TerritorialEntryKind.Municipality => Municipality.Create(name),
            TerritorialEntryKind.Settlement => Settlement.Create(await MunicipalityAsync(RequiredParentId(parentId), cancellationToken), name),
            TerritorialEntryKind.District => District.Create(await SettlementAsync(RequiredParentId(parentId), cancellationToken), name),
            TerritorialEntryKind.Quarter => Quarter.Create(await SettlementAsync(RequiredParentId(parentId), cancellationToken), name),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        StampCreated(entry);
        dbContext.Add(entry);
        await SaveAsync("A territorial entry with this name already exists for the selected parent.", cancellationToken);
        return entry.Id;
    }

    public async Task UpdateAsync(TerritorialEntryKind kind, Guid id, string name, bool isActive, CancellationToken cancellationToken)
    {
        switch (kind)
        {
            case TerritorialEntryKind.Municipality:
                await UpdateAsync(await MunicipalityAsync(id, cancellationToken), name, isActive, cancellationToken); break;
            case TerritorialEntryKind.Settlement:
                await UpdateAsync(await SettlementAsync(id, cancellationToken), name, isActive, cancellationToken); break;
            case TerritorialEntryKind.District:
                await UpdateAsync(await DistrictAsync(id, cancellationToken), name, isActive, cancellationToken); break;
            case TerritorialEntryKind.Quarter:
                await UpdateAsync(await QuarterAsync(id, cancellationToken), name, isActive, cancellationToken); break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public async Task<IReadOnlyList<TerritorialEntryDto>> GetEntriesAsync(TerritorialEntriesQuery request, CancellationToken cancellationToken)
    {
        var search = Normalize(request.SearchTerm);
        return request.Kind switch
        {
            TerritorialEntryKind.Municipality => await dbContext.Municipalities.AsNoTracking()
                .Where(x => request.IncludeInactive || x.IsActive)
                .Where(x => search.Length == 0 || x.NormalizedName.Contains(search))
                .OrderBy(x => x.Name).Select(x => new TerritorialEntryDto(x.Id, x.Name, x.IsActive, null)).ToListAsync(cancellationToken),
            TerritorialEntryKind.Settlement => await dbContext.Settlements.AsNoTracking()
                .Where(x => x.MunicipalityId == request.ParentId && (request.IncludeInactive || x.IsActive))
                .Where(x => search.Length == 0 || x.NormalizedName.Contains(search))
                .OrderBy(x => x.Name).Select(x => new TerritorialEntryDto(x.Id, x.Name, x.IsActive, x.MunicipalityId)).ToListAsync(cancellationToken),
            TerritorialEntryKind.District => await dbContext.Districts.AsNoTracking()
                .Where(x => x.SettlementId == request.ParentId && (request.IncludeInactive || x.IsActive))
                .Where(x => search.Length == 0 || x.NormalizedName.Contains(search))
                .OrderBy(x => x.Name).Select(x => new TerritorialEntryDto(x.Id, x.Name, x.IsActive, x.SettlementId)).ToListAsync(cancellationToken),
            TerritorialEntryKind.Quarter => await dbContext.Quarters.AsNoTracking()
                .Where(x => x.SettlementId == request.ParentId && (request.IncludeInactive || x.IsActive))
                .Where(x => search.Length == 0 || x.NormalizedName.Contains(search))
                .OrderBy(x => x.Name).Select(x => new TerritorialEntryDto(x.Id, x.Name, x.IsActive, x.SettlementId)).ToListAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Kind))
        };
    }

    public async Task DeleteAsync(TerritorialEntryKind kind, Guid id, CancellationToken cancellationToken)
    {
        switch (kind)
        {
            case TerritorialEntryKind.Municipality:
                if (await dbContext.Settlements.AnyAsync(x => x.MunicipalityId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This Municipality has Settlements. Delete its child entries first, or deactivate it.");
                if (await dbContext.Set<PersonAddress>().AnyAsync(x => x.MunicipalityId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This Municipality is referenced by person addresses and cannot be deleted. Deactivate it instead.");
                dbContext.Municipalities.Remove(await MunicipalityAsync(id, cancellationToken));
                break;
            case TerritorialEntryKind.Settlement:
                if (await dbContext.Districts.AnyAsync(x => x.SettlementId == id, cancellationToken)
                    || await dbContext.Quarters.AnyAsync(x => x.SettlementId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This Settlement has Districts or Quarters. Delete its child entries first, or deactivate it.");
                if (await dbContext.Set<PersonAddress>().AnyAsync(x => x.SettlementId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This Settlement is referenced by person addresses and cannot be deleted. Deactivate it instead.");
                dbContext.Settlements.Remove(await SettlementAsync(id, cancellationToken));
                break;
            case TerritorialEntryKind.District:
                if (await dbContext.Set<PersonAddress>().AnyAsync(x => x.DistrictId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This District is referenced by person addresses and cannot be deleted. Deactivate it instead.");
                dbContext.Districts.Remove(await DistrictAsync(id, cancellationToken));
                break;
            case TerritorialEntryKind.Quarter:
                if (await dbContext.Set<PersonAddress>().AnyAsync(x => x.QuarterId == id, cancellationToken))
                    throw new TerritorialStructureConflictException("This Quarter is referenced by person addresses and cannot be deleted. Deactivate it instead.");
                dbContext.Quarters.Remove(await QuarterAsync(id, cancellationToken));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }

        await SaveAsync("The territorial entry could not be deleted because it is still referenced.", cancellationToken);
    }

    private async Task UpdateAsync(Municipality entry, string name, bool active, CancellationToken ct) { entry.Rename(name); if (active) entry.Activate(); else entry.Deactivate(); StampModified(entry); await SaveAsync("A Municipality with this name already exists.", ct); }
    private async Task UpdateAsync(Settlement entry, string name, bool active, CancellationToken ct) { entry.Rename(name); if (active) entry.Activate(); else entry.Deactivate(); StampModified(entry); await SaveAsync("A Settlement with this name already exists.", ct); }
    private async Task UpdateAsync(District entry, string name, bool active, CancellationToken ct) { entry.Rename(name); if (active) entry.Activate(); else entry.Deactivate(); StampModified(entry); await SaveAsync("A District with this name already exists.", ct); }
    private async Task UpdateAsync(Quarter entry, string name, bool active, CancellationToken ct) { entry.Rename(name); if (active) entry.Activate(); else entry.Deactivate(); StampModified(entry); await SaveAsync("A Quarter with this name already exists.", ct); }

    private async Task<Municipality> MunicipalityAsync(Guid id, CancellationToken ct) { var entity = await dbContext.Municipalities.SingleOrDefaultAsync(x => x.Id == id, ct); return Guard.Against.NotFound(id, entity); }
    private async Task<Settlement> SettlementAsync(Guid id, CancellationToken ct) { var entity = await dbContext.Settlements.SingleOrDefaultAsync(x => x.Id == id, ct); return Guard.Against.NotFound(id, entity); }
    private async Task<District> DistrictAsync(Guid id, CancellationToken ct) { var entity = await dbContext.Districts.SingleOrDefaultAsync(x => x.Id == id, ct); return Guard.Against.NotFound(id, entity); }
    private async Task<Quarter> QuarterAsync(Guid id, CancellationToken ct) { var entity = await dbContext.Quarters.SingleOrDefaultAsync(x => x.Id == id, ct); return Guard.Against.NotFound(id, entity); }
    private static Guid RequiredParentId(Guid? id) => id ?? throw new ArgumentException("A parent ID is required.", nameof(id));
    private async Task SaveAsync(string message, CancellationToken ct) { try { await dbContext.SaveChangesAsync(ct); } catch (DbUpdateException ex) { throw new TerritorialStructureConflictException(message, ex); } }
    private void StampCreated(BaseAuditableEntity entity) { var now = DateTimeOffset.UtcNow; entity.Created = now; entity.LastModified = now; entity.CreatedBy = UserId(); entity.LastModifiedBy = UserId(); }
    private void StampModified(BaseAuditableEntity entity) { entity.LastModified = DateTimeOffset.UtcNow; entity.LastModifiedBy = UserId(); }
    private string UserId() => user.Id ?? throw new UnauthorizedAccessException();
    private static string Normalize(string? value) => string.Join(" ", (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
}
