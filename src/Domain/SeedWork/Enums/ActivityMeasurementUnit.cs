namespace Domain.SeedWork.Enums;

public enum ActivityMeasurementUnit
{
    Piece,
    Centimeter,
    Meter,
    SquareMeter,
    CubicMeter
}

public static class ActivityMeasurementUnitCodes
{
    public static string ToCode(this ActivityMeasurementUnit unit) =>
        unit switch
        {
            ActivityMeasurementUnit.Piece => "piece",
            ActivityMeasurementUnit.Centimeter => "cm",
            ActivityMeasurementUnit.Meter => "m",
            ActivityMeasurementUnit.SquareMeter => "m2",
            ActivityMeasurementUnit.CubicMeter => "m3",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        };

    public static bool TryParse(string? value, out ActivityMeasurementUnit unit)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();
        unit = normalizedValue switch
        {
            "piece" => ActivityMeasurementUnit.Piece,
            "cm" => ActivityMeasurementUnit.Centimeter,
            "m" => ActivityMeasurementUnit.Meter,
            "m2" => ActivityMeasurementUnit.SquareMeter,
            "m3" => ActivityMeasurementUnit.CubicMeter,
            _ => default
        };

        return normalizedValue is "piece" or "cm" or "m" or "m2" or "m3";
    }
}
