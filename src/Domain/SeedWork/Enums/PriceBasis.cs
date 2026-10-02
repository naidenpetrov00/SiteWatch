namespace Domain.SeedWork.Enums;

public enum PriceBasis
{
    Item = 0,
    Package = 1
}

public static class PriceBasisCodes
{
    public static string ToCode(this PriceBasis basis) => basis switch
    {
        PriceBasis.Item => "item",
        PriceBasis.Package => "package",
        _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, null)
    };

    public static bool TryParse(string? value, out PriceBasis basis)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        basis = normalized switch
        {
            "item" => PriceBasis.Item,
            "package" => PriceBasis.Package,
            _ => default
        };
        return normalized is "item" or "package";
    }
}
