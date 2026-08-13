using Domain.SeedWork;
using Domain.SeedWork.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.ValueObjects;

public sealed class SiteMediaPolicy : ValueObject
{
    public const int MaxCategoryLength = 50;
    public const int MaxCategoryCount = 20;
    public const string OtherCategory = "Other";
    public const string AllFilter = "All";

    private static readonly IReadOnlyList<SiteMediaPolicyPresetDefinition> Definitions =
    [
        new(
            MediaPolicyPreset.ApartmentRenovation,
            "Apartment Renovation",
            ["Design", "Demolition", "Electricity", "Pipes", "Finishes", OtherCategory]),
        new(
            MediaPolicyPreset.HouseBuild,
            "House Build",
            ["Design", "Foundation", "Structure", "Roof", "Electricity", "Pipes", "Exterior", OtherCategory]),
        new(
            MediaPolicyPreset.CommercialBuild,
            "Commercial Build",
            ["Design", "Structure", "Electricity", "HVAC", "Fire Safety", "Finishes", OtherCategory]),
        new(
            MediaPolicyPreset.SiteMaintenance,
            "Site Maintenance",
            ["Electricity", "Pipes", "HVAC", "Repairs", "Safety", OtherCategory]),
        new(MediaPolicyPreset.Custom, "Custom", [OtherCategory]),
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private SiteMediaPolicy()
    {
    }

    private SiteMediaPolicy(MediaPolicyPreset preset, IEnumerable<string> categories)
    {
        Preset = preset;
        Categories = Normalize(categories);
    }

    public MediaPolicyPreset Preset { get; private set; }
    public IReadOnlyList<string> Categories { get; private set; } = [OtherCategory];

    public static IReadOnlyList<SiteMediaPolicyPresetDefinition> PresetDefinitions => Definitions;

    public static SiteMediaPolicy FromPreset(MediaPolicyPreset preset)
    {
        var definition = GetDefinition(preset);
        return new SiteMediaPolicy(preset, definition.Categories);
    }

    public static bool TryParsePreset(string? value, out MediaPolicyPreset preset)
    {
        preset = default;
        var normalizedValue = value?.Trim();
        return normalizedValue is not null
            && Enum.GetNames<MediaPolicyPreset>()
                .Contains(normalizedValue, StringComparer.OrdinalIgnoreCase)
            && Enum.TryParse(normalizedValue, true, out preset);
    }

    public static SiteMediaPolicy Create(MediaPolicyPreset requestedPreset, IEnumerable<string> categories)
    {
        var definition = GetDefinition(requestedPreset);
        var normalizedCategories = Normalize(categories);

        if (requestedPreset == MediaPolicyPreset.Custom)
        {
            return new SiteMediaPolicy(MediaPolicyPreset.Custom, normalizedCategories);
        }

        var matchesPreset = normalizedCategories.Count == definition.Categories.Count
            && normalizedCategories.All(category =>
                definition.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));

        return matchesPreset
            ? new SiteMediaPolicy(requestedPreset, definition.Categories)
            : new SiteMediaPolicy(MediaPolicyPreset.Custom, normalizedCategories);
    }

    public bool AllowsCategory(string? category) => ResolveCategory(category) is not null;

    public string? ResolveCategory(string? category)
    {
        var normalizedCategory = NormalizeCategory(category);
        return normalizedCategory is null
            ? null
            : Categories.FirstOrDefault(
                savedCategory => string.Equals(savedCategory, normalizedCategory, StringComparison.OrdinalIgnoreCase));
    }

    public void AddCategories(IEnumerable<string> categories)
    {
        Categories = Normalize(Categories.Concat(categories));
    }

    public string ToStorageValue()
    {
        var storage = new SiteMediaPolicyStorage(Preset, Categories.ToArray());

        return JsonSerializer.Serialize(storage, SerializerOptions);
    }

    public static SiteMediaPolicy FromStorageValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FromPreset(MediaPolicyPreset.Custom);
        }

        var storage = JsonSerializer.Deserialize<SiteMediaPolicyStorage>(value, SerializerOptions)
            ?? throw new InvalidOperationException("Could not deserialize site media policy.");

        return new SiteMediaPolicy(storage.Preset, storage.Categories ?? []);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Preset;

        foreach (var category in Categories)
        {
            yield return category.ToUpperInvariant();
        }
    }

    public static string? NormalizeCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var categories = new List<string>();
        foreach (var value in values)
        {
            var category = NormalizeCategory(value);
            if (category is null
                || string.Equals(category, OtherCategory, StringComparison.OrdinalIgnoreCase)
                || categories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(category, AllFilter, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"{AllFilter} is reserved for the media filter that shows every item.",
                    nameof(values));
            }

            if (category.Length > MaxCategoryLength)
            {
                throw new ArgumentException(
                    $"Media categories cannot exceed {MaxCategoryLength} characters.",
                    nameof(values));
            }

            categories.Add(category);
        }

        categories.Add(OtherCategory);
        if (categories.Count > MaxCategoryCount)
        {
            throw new ArgumentException(
                $"A media policy cannot contain more than {MaxCategoryCount} categories.",
                nameof(values));
        }

        return categories;
    }

    private static SiteMediaPolicyPresetDefinition GetDefinition(MediaPolicyPreset preset) =>
        Definitions.FirstOrDefault(definition => definition.Preset == preset)
        ?? throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unsupported media policy preset.");

    private sealed record SiteMediaPolicyStorage(MediaPolicyPreset Preset, string[]? Categories);
}

public sealed record SiteMediaPolicyPresetDefinition(
    MediaPolicyPreset Preset,
    string DisplayName,
    IReadOnlyList<string> Categories);
