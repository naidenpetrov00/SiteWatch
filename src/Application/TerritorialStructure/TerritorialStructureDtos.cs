namespace Application.TerritorialStructure;

public enum TerritorialEntryKind
{
    Municipality,
    Settlement,
    District,
    Quarter
}

public sealed record TerritorialEntryDto(
    Guid Id,
    string Name,
    bool IsActive,
    Guid? ParentId);
