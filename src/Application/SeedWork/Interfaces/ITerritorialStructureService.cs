using Application.TerritorialStructure;

namespace Application.SeedWork.Interfaces;

public interface ITerritorialStructureService
{
    Task<Guid> CreateAsync(TerritorialEntryKind kind, Guid? parentId, string name, CancellationToken cancellationToken);
    Task UpdateAsync(TerritorialEntryKind kind, Guid id, string name, bool isActive, CancellationToken cancellationToken);
    Task DeleteAsync(TerritorialEntryKind kind, Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TerritorialEntryDto>> GetEntriesAsync(TerritorialEntriesQuery request, CancellationToken cancellationToken);
}
