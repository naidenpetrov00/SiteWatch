namespace Domain.SeedWork.Enums;

public enum ProductPackageUnit
{
    Piece,
    Pack,
    Box,
    Set,
    Kilogram,
    Gram,
    Liter,
    Milliliter,
    Meter,
    Centimeter,
    SquareMeter,
    CubicMeter,
    Roll,
    Bag
}

public static class ProductPackageUnitCodes
{
    public static string ToCode(this ProductPackageUnit unit) =>
        unit switch
        {
            ProductPackageUnit.Piece => "piece",
            ProductPackageUnit.Pack => "pack",
            ProductPackageUnit.Box => "box",
            ProductPackageUnit.Set => "set",
            ProductPackageUnit.Kilogram => "kg",
            ProductPackageUnit.Gram => "g",
            ProductPackageUnit.Liter => "l",
            ProductPackageUnit.Milliliter => "ml",
            ProductPackageUnit.Meter => "m",
            ProductPackageUnit.Centimeter => "cm",
            ProductPackageUnit.SquareMeter => "m2",
            ProductPackageUnit.CubicMeter => "m3",
            ProductPackageUnit.Roll => "roll",
            ProductPackageUnit.Bag => "bag",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };

    public static bool TryParse(string? value, out ProductPackageUnit unit)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();
        unit = normalizedValue switch
        {
            "piece" => ProductPackageUnit.Piece,
            "pack" => ProductPackageUnit.Pack,
            "box" => ProductPackageUnit.Box,
            "set" => ProductPackageUnit.Set,
            "kg" => ProductPackageUnit.Kilogram,
            "g" => ProductPackageUnit.Gram,
            "l" => ProductPackageUnit.Liter,
            "ml" => ProductPackageUnit.Milliliter,
            "m" => ProductPackageUnit.Meter,
            "cm" => ProductPackageUnit.Centimeter,
            "m2" => ProductPackageUnit.SquareMeter,
            "m3" => ProductPackageUnit.CubicMeter,
            "roll" => ProductPackageUnit.Roll,
            "bag" => ProductPackageUnit.Bag,
            _ => default
        };

        return normalizedValue is
            "piece" or "pack" or "box" or "set" or "kg" or "g" or "l" or "ml"
            or "m" or "cm" or "m2" or "m3" or "roll" or "bag";
    }
}
