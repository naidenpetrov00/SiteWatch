namespace Domain.Entities;

internal static class TerritorialName
{
    public static (string Name, string NormalizedName) Normalize(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A name is required.", parameterName);
        var name = string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (name.Length > Municipality.MaxNameLength) throw new ArgumentOutOfRangeException(parameterName, name.Length, "A name cannot exceed 100 characters.");
        return (name, name.ToUpperInvariant());
    }
}
