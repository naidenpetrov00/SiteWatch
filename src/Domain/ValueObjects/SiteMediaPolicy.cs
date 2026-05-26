using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using System.Text.Json;

namespace Domain.ValueObjects;

public sealed class SiteMediaPolicy : ValueObject
{
    private static readonly ImageCategory[] RegularImageCategories =
    [
        ImageCategory.Pipes,
        ImageCategory.Electricity,
        ImageCategory.Design,
    ];

    private static readonly VideoCategory[] RegularVideoCategories =
    [
        VideoCategory.Pipes,
        VideoCategory.Electricity,
        VideoCategory.Design,
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private SiteMediaPolicy()
    {
    }

    private SiteMediaPolicy(
        MediaPolicyPreset preset,
        IEnumerable<ImageCategory> allowedImageCategories,
        IEnumerable<VideoCategory> allowedVideoCategories)
    {
        Preset = preset;
        AllowedImageCategories = Normalize(allowedImageCategories);
        AllowedVideoCategories = Normalize(allowedVideoCategories);
    }

    public MediaPolicyPreset Preset { get; private set; }
    public IReadOnlyCollection<ImageCategory> AllowedImageCategories { get; private set; } = [];
    public IReadOnlyCollection<VideoCategory> AllowedVideoCategories { get; private set; } = [];

    public static SiteMediaPolicy Regular() => new(
        MediaPolicyPreset.Regular,
        RegularImageCategories,
        RegularVideoCategories);

    public static SiteMediaPolicy Custom(
        IEnumerable<ImageCategory> allowedImageCategories,
        IEnumerable<VideoCategory> allowedVideoCategories) => new(
        MediaPolicyPreset.Custom,
        allowedImageCategories,
        allowedVideoCategories);

    public bool AllowsImageCategory(ImageCategory category) => AllowedImageCategories.Contains(category);

    public bool AllowsVideoCategory(VideoCategory category) => AllowedVideoCategories.Contains(category);

    public string ToStorageValue()
    {
        var storage = new SiteMediaPolicyStorage(
            Preset,
            AllowedImageCategories.ToArray(),
            AllowedVideoCategories.ToArray());

        return JsonSerializer.Serialize(storage, SerializerOptions);
    }

    public static SiteMediaPolicy FromStorageValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Regular();
        }

        var storage = JsonSerializer.Deserialize<SiteMediaPolicyStorage>(value, SerializerOptions)
            ?? throw new InvalidOperationException("Could not deserialize site media policy.");

        return new SiteMediaPolicy(
            storage.Preset,
            storage.AllowedImageCategories ?? [],
            storage.AllowedVideoCategories ?? []);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Preset;

        foreach (var imageCategory in AllowedImageCategories)
        {
            yield return imageCategory;
        }

        yield return "|";

        foreach (var videoCategory in AllowedVideoCategories)
        {
            yield return videoCategory;
        }
    }

    private static IReadOnlyCollection<TEnum> Normalize<TEnum>(IEnumerable<TEnum> values)
        where TEnum : struct, Enum
    {
        Guard.Against.Null(values, nameof(values));
        return values.Distinct().OrderBy(value => value).ToArray();
    }

    private sealed record SiteMediaPolicyStorage(
        MediaPolicyPreset Preset,
        ImageCategory[]? AllowedImageCategories,
        VideoCategory[]? AllowedVideoCategories);
}
