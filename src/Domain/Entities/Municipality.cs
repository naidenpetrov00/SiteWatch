using Domain.SeedWork;

namespace Domain.Entities;

public sealed class Municipality : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxNameLength = 100;

    private Municipality() { }

    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public static Municipality Create(string name)
    {
        var municipality = new Municipality { Id = Guid.NewGuid() };
        municipality.Rename(name);
        return municipality;
    }

    public void Rename(string name) => (Name, NormalizedName) = TerritorialName.Normalize(name, nameof(name));
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
