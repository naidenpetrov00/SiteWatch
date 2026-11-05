using Domain.SeedWork;

namespace Domain.Entities;

public sealed class District : BaseAuditableEntity, IAgregateRoot
{
    private District() { }

    public Guid SettlementId { get; private set; }
    public Settlement Settlement { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public static District Create(Settlement settlement, string name)
    {
        ArgumentNullException.ThrowIfNull(settlement);
        var district = new District { Id = Guid.NewGuid(), Settlement = settlement, SettlementId = settlement.Id };
        district.Rename(name);
        return district;
    }

    public void Rename(string name) => (Name, NormalizedName) = TerritorialName.Normalize(name, nameof(name));
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
