namespace Domain.SeedWork.Enums;

public enum ProductQuantityBehavior
{
    /// <summary>
    /// Configured quantity is multiplied by requested measurement divided by the
    /// requirement section's basis quantity.
    /// </summary>
    Proportional,

    /// <summary>Configured quantity remains unchanged for every requested measurement.</summary>
    Fixed
}

public static class ProductQuantityBehaviorCodes
{
    public static string ToCode(this ProductQuantityBehavior behavior) =>
        behavior switch
        {
            ProductQuantityBehavior.Proportional => "proportional",
            ProductQuantityBehavior.Fixed => "fixed",
            _ => throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null)
        };

    public static bool TryParse(string? value, out ProductQuantityBehavior behavior)
    {
        var normalizedValue = value?.Trim().ToLowerInvariant();
        behavior = normalizedValue switch
        {
            "proportional" => ProductQuantityBehavior.Proportional,
            "fixed" => ProductQuantityBehavior.Fixed,
            _ => default
        };

        return normalizedValue is "proportional" or "fixed";
    }
}
