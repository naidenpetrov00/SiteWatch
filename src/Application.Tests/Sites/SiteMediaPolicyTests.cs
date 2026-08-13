using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Application.Tests.Sites;

public sealed class SiteMediaPolicyTests
{
    public static TheoryData<MediaPolicyPreset, string, string[]> Presets => new()
    {
        {
            MediaPolicyPreset.ApartmentRenovation,
            "Apartment Renovation",
            ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"]
        },
        {
            MediaPolicyPreset.HouseBuild,
            "House Build",
            ["Design", "Foundation", "Structure", "Roof", "Electricity", "Pipes", "Exterior", "Other"]
        },
        {
            MediaPolicyPreset.CommercialBuild,
            "Commercial Build",
            ["Design", "Structure", "Electricity", "HVAC", "Fire Safety", "Finishes", "Other"]
        },
        {
            MediaPolicyPreset.SiteMaintenance,
            "Site Maintenance",
            ["Electricity", "Pipes", "HVAC", "Repairs", "Safety", "Other"]
        },
        { MediaPolicyPreset.Custom, "Custom", ["Other"] },
    };

    [Theory]
    [MemberData(nameof(Presets))]
    public void Presets_expose_the_expected_shared_media_categories(
        MediaPolicyPreset preset,
        string displayName,
        string[] categories)
    {
        var definition = Assert.Single(
            SiteMediaPolicy.PresetDefinitions,
            item => item.Preset == preset);

        Assert.Equal(displayName, definition.DisplayName);
        Assert.Equal(categories, definition.Categories);
        Assert.Equal(categories, SiteMediaPolicy.FromPreset(preset).Categories);
    }

    [Fact]
    public void Create_retains_a_preset_when_categories_match_ignoring_case_order_and_whitespace()
    {
        var policy = SiteMediaPolicy.Create(
            MediaPolicyPreset.ApartmentRenovation,
            [" other ", "pipes", "ELECTRICITY", "Finishes", " Design ", "Demolition"]);

        Assert.Equal(MediaPolicyPreset.ApartmentRenovation, policy.Preset);
        Assert.Equal(
            ["Design", "Demolition", "Electricity", "Pipes", "Finishes", "Other"],
            policy.Categories);
    }

    [Fact]
    public void Custom_policy_normalizes_categories_and_keeps_other_once_at_the_end()
    {
        var policy = SiteMediaPolicy.Create(
            MediaPolicyPreset.Custom,
            ["  HVAC   Controls ", "hvac controls", "Other", "  ", "Safety"]);

        Assert.Equal(MediaPolicyPreset.Custom, policy.Preset);
        Assert.Equal(["HVAC Controls", "Safety", "Other"], policy.Categories);
        Assert.Equal("HVAC Controls", policy.ResolveCategory("  hvac   controls "));
        Assert.False(policy.AllowsCategory("Design"));
    }

    [Fact]
    public void Adding_a_new_category_switches_a_preset_to_custom_but_duplicate_additions_are_a_no_op()
    {
        var unchanged = SiteMediaPolicy.FromPreset(MediaPolicyPreset.SiteMaintenance);
        unchanged.AddCategories([" hvac ", "OTHER"]);

        Assert.Equal(MediaPolicyPreset.SiteMaintenance, unchanged.Preset);

        unchanged.AddCategories(["Access Control"]);

        Assert.Equal(MediaPolicyPreset.Custom, unchanged.Preset);
        Assert.Equal(
            ["Electricity", "Pipes", "HVAC", "Repairs", "Safety", "Access Control", "Other"],
            unchanged.Categories);
    }

    [Fact]
    public void Category_limits_accept_the_exact_boundaries()
    {
        var fiftyCharacters = new string('x', SiteMediaPolicy.MaxCategoryLength);
        var nineteenCategories = Enumerable.Range(1, SiteMediaPolicy.MaxCategoryCount - 2)
            .Select(index => $"Category {index}")
            .Append(fiftyCharacters);

        var policy = SiteMediaPolicy.Create(MediaPolicyPreset.Custom, nineteenCategories);

        Assert.Equal(SiteMediaPolicy.MaxCategoryCount, policy.Categories.Count);
        Assert.Contains(fiftyCharacters, policy.Categories);
        Assert.Equal(SiteMediaPolicy.OtherCategory, policy.Categories[^1]);
    }

    [Fact]
    public void Category_rules_reject_reserved_oversized_and_excess_values()
    {
        Assert.Throws<ArgumentException>(() =>
            SiteMediaPolicy.Create(MediaPolicyPreset.Custom, ["All"]));
        Assert.Throws<ArgumentException>(() =>
            SiteMediaPolicy.Create(
                MediaPolicyPreset.Custom,
                [new string('x', SiteMediaPolicy.MaxCategoryLength + 1)]));
        Assert.Throws<ArgumentException>(() =>
            SiteMediaPolicy.Create(
                MediaPolicyPreset.Custom,
                Enumerable.Range(1, SiteMediaPolicy.MaxCategoryCount)
                    .Select(index => $"Category {index}")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Media_entities_reject_empty_categories(string? category)
    {
        Assert.Throws<ArgumentException>(() =>
            new SiteImage(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), category!));
        Assert.Throws<ArgumentException>(() =>
            new SiteVideo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 12, category!));
    }

    [Fact]
    public void Media_entities_normalize_categories_and_enforce_the_length_limit()
    {
        var image = new SiteImage(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  HVAC   Controls ");
        var video = new SiteVideo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 12, "  Fire   Safety ");

        Assert.Equal("HVAC Controls", image.Category);
        Assert.Equal("Fire Safety", video.Category);
        Assert.Throws<ArgumentException>(() => image.ChangeCategory(new string('x', 51)));
        Assert.Throws<ArgumentException>(() => video.ChangeCategory(new string('x', 51)));
    }
}
