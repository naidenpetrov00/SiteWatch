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

    private static readonly FileCategory[] RegularFileCategories =
    [
        FileCategory.Pipes,
        FileCategory.Electricity,
        FileCategory.Design,
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private SiteMediaPolicy()
    {
    }

    private SiteMediaPolicy(
        MediaPolicyPreset preset,
        IEnumerable<ImageCategory> allowedImageCategories,
        IEnumerable<VideoCategory> allowedVideoCategories,
        IEnumerable<FileCategory> allowedFileCategories)
    {
        Preset = preset;
        AllowedImageCategories = Normalize(allowedImageCategories);
        AllowedVideoCategories = Normalize(allowedVideoCategories);
        AllowedFileCategories = Normalize(allowedFileCategories);
    }

    public MediaPolicyPreset Preset { get; private set; }
    public IReadOnlyCollection<ImageCategory> AllowedImageCategories { get; private set; } = [];
    public IReadOnlyCollection<VideoCategory> AllowedVideoCategories { get; private set; } = [];
    public IReadOnlyCollection<FileCategory> AllowedFileCategories { get; private set; } = [];

    public static SiteMediaPolicy Regular() => new(
        MediaPolicyPreset.Regular,
        RegularImageCategories,
        RegularVideoCategories,
        RegularFileCategories);

    public static SiteMediaPolicy Custom(
        IEnumerable<ImageCategory> allowedImageCategories,
        IEnumerable<VideoCategory> allowedVideoCategories,
        IEnumerable<FileCategory> allowedFileCategories) => new(
        MediaPolicyPreset.Custom,
        allowedImageCategories,
        allowedVideoCategories,
        allowedFileCategories);

    public bool AllowsImageCategory(ImageCategory category) => AllowedImageCategories.Contains(category);

    public bool AllowsVideoCategory(VideoCategory category) => AllowedVideoCategories.Contains(category);

    public bool AllowsFileCategory(FileCategory category) => AllowedFileCategories.Contains(category);

    public void ChangePreset(MediaPolicyPreset preset) => Preset = preset;

    public string ToStorageValue()
    {
        var storage = new SiteMediaPolicyStorage(
            Preset,
            AllowedImageCategories.ToArray(),
            AllowedVideoCategories.ToArray(),
            AllowedFileCategories.ToArray());

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
            storage.AllowedVideoCategories ?? [],
            storage.AllowedFileCategories
                ?? (storage.Preset == MediaPolicyPreset.Regular ? RegularFileCategories : []));
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

        yield return "|";

        foreach (var fileCategory in AllowedFileCategories)
        {
            yield return fileCategory;
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
        VideoCategory[]? AllowedVideoCategories,
        FileCategory[]? AllowedFileCategories);
}
