namespace Domain.SeedWork.Enums;

public enum ActivityPricingMode
{
    Fixed = 0,
    PerMeasurement = 1,
    Free = 2
}

public static class ActivityPricingModeCodes
{
    public static string ToCode(this ActivityPricingMode mode) => mode switch
    {
        ActivityPricingMode.Fixed => "fixed",
        ActivityPricingMode.PerMeasurement => "per-measurement",
        ActivityPricingMode.Free => "free",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public static bool TryParse(string? value, out ActivityPricingMode mode)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        mode = normalized switch
        {
            "fixed" => ActivityPricingMode.Fixed,
            "per-measurement" => ActivityPricingMode.PerMeasurement,
            "free" => ActivityPricingMode.Free,
            _ => default
        };

        return normalized is "fixed" or "per-measurement" or "free";
    }
}
