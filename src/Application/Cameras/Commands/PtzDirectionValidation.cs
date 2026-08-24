namespace Application.Cameras.Commands;

internal static class PtzDirectionValidation
{
    private static readonly HashSet<string> SupportedDirections =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Up",
            "Down",
            "Left",
            "Right",
        };

    public static bool IsSupported(string? direction) =>
        !string.IsNullOrWhiteSpace(direction) && SupportedDirections.Contains(direction.Trim());

    public static string Normalize(string direction) => direction.Trim().ToUpperInvariant() switch
    {
        "UP" => "Up",
        "DOWN" => "Down",
        "LEFT" => "Left",
        "RIGHT" => "Right",
        _ => direction,
    };
}
