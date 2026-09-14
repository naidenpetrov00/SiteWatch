using System.Text.Json;
using Domain.SeedWork;

namespace Domain.ValueObjects;

public sealed class ProductSearchConfiguration : ValueObject
{
    public const int MaxCollectionCount = 20;
    public const int MaxEntryLength = 200;
    public const int MaxPrimarySearchPhraseLength = 250;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private ProductSearchConfiguration()
    {
    }

    private ProductSearchConfiguration(
        string? primarySearchPhrase,
        IEnumerable<string> alternativeSearchPhrases,
        IEnumerable<string> requiredKeywords,
        IEnumerable<string> excludedKeywords)
    {
        PrimarySearchPhrase = NormalizeOptional(primarySearchPhrase);
        if (PrimarySearchPhrase?.Length > MaxPrimarySearchPhraseLength)
        {
            throw new ArgumentException(
                $"Primary search phrase cannot exceed {MaxPrimarySearchPhraseLength} characters.",
                nameof(primarySearchPhrase));
        }

        AlternativeSearchPhrases = NormalizeCollection(alternativeSearchPhrases);
        RequiredKeywords = NormalizeCollection(requiredKeywords);
        ExcludedKeywords = NormalizeCollection(excludedKeywords);
    }

    public string? PrimarySearchPhrase { get; private set; }
    public IReadOnlyList<string> AlternativeSearchPhrases { get; private set; } = [];
    public IReadOnlyList<string> RequiredKeywords { get; private set; } = [];
    public IReadOnlyList<string> ExcludedKeywords { get; private set; } = [];

    public static ProductSearchConfiguration Create(
        string? primarySearchPhrase,
        IEnumerable<string>? alternativeSearchPhrases = null,
        IEnumerable<string>? requiredKeywords = null,
        IEnumerable<string>? excludedKeywords = null) =>
        new(
            primarySearchPhrase,
            alternativeSearchPhrases ?? [],
            requiredKeywords ?? [],
            excludedKeywords ?? []);

    public string ResolvePrimarySearchPhrase(string title, string? brand, string? model)
    {
        var primarySearchPhrase = PrimarySearchPhrase;
        if (!string.IsNullOrWhiteSpace(primarySearchPhrase))
        {
            return primarySearchPhrase;
        }

        return string.Join(
            " ",
            new[] { title, brand, model }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    public string ToStorageValue() =>
        JsonSerializer.Serialize(
            new ProductSearchConfigurationStorage(
                PrimarySearchPhrase,
                AlternativeSearchPhrases.ToArray(),
                RequiredKeywords.ToArray(),
                ExcludedKeywords.ToArray()),
            SerializerOptions);

    public static ProductSearchConfiguration FromStorageValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Create(null);
        }

        var storage = JsonSerializer.Deserialize<ProductSearchConfigurationStorage>(
            value,
            SerializerOptions)
            ?? throw new InvalidOperationException("Could not deserialize product search configuration.");

        return Create(
            storage.PrimarySearchPhrase,
            storage.AlternativeSearchPhrases,
            storage.RequiredKeywords,
            storage.ExcludedKeywords);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return (PrimarySearchPhrase ?? string.Empty).ToUpperInvariant();
        yield return "ALTERNATIVE";

        foreach (var value in AlternativeSearchPhrases)
        {
            yield return value.ToUpperInvariant();
        }

        yield return "REQUIRED";
        foreach (var value in RequiredKeywords)
        {
            yield return value.ToUpperInvariant();
        }

        yield return "EXCLUDED";
        foreach (var value in ExcludedKeywords)
        {
            yield return value.ToUpperInvariant();
        }
    }

    private static IReadOnlyList<string> NormalizeCollection(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var normalizedValues = values
            .Select(NormalizeOptional)
            .Where(value => value is not null)
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedValues.Count > MaxCollectionCount)
        {
            throw new ArgumentException(
                $"A search collection cannot contain more than {MaxCollectionCount} entries.",
                nameof(values));
        }

        if (normalizedValues.Any(value => value.Length > MaxEntryLength))
        {
            throw new ArgumentException(
                $"Search entries cannot exceed {MaxEntryLength} characters.",
                nameof(values));
        }

        return normalizedValues;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed record ProductSearchConfigurationStorage(
        string? PrimarySearchPhrase,
        string[]? AlternativeSearchPhrases,
        string[]? RequiredKeywords,
        string[]? ExcludedKeywords);
}
