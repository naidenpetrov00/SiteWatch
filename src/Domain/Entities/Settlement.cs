using Domain.SeedWork;

namespace Domain.Entities;

public sealed class Settlement : BaseAuditableEntity, IAgregateRoot
{
    private Settlement() { }

    public Guid MunicipalityId { get; private set; }
    public Municipality Municipality { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public static Settlement Create(Municipality municipality, string name)
    {
        ArgumentNullException.ThrowIfNull(municipality);
        var settlement = new Settlement { Id = Guid.NewGuid(), Municipality = municipality, MunicipalityId = municipality.Id };
        settlement.Rename(name);
        return settlement;
    }

    public void Rename(string name) => (Name, NormalizedName) = TerritorialName.Normalize(name, nameof(name));
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
