using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Infrastructure.Tests.Sites;

public sealed class SiteMediaPersistenceMetadataTests
{
    [Theory]
    [InlineData(typeof(SiteImage))]
    [InlineData(typeof(SiteVideo))]
    public void Media_categories_are_required_nvarchar_50_columns(Type entityType)
    {
        using var dbContext = CreateDbContext();

        var property = dbContext.Model.FindEntityType(entityType)!
            .FindProperty(nameof(SiteImage.Category))!;

        Assert.False(property.IsNullable);
        Assert.Equal(SiteMediaPolicy.MaxCategoryLength, property.GetMaxLength());
        Assert.Equal("nvarchar(50)", property.GetColumnType());
    }

    [Fact]
    public void Site_media_policy_converter_round_trips_string_categories_and_preset()
    {
        using var dbContext = CreateDbContext();
        var property = dbContext.Model.FindEntityType(typeof(Site))!
            .FindProperty(nameof(Site.MediaPolicy))!;
        var converter = property.GetValueConverter()!;
        var policy = SiteMediaPolicy.Create(
            MediaPolicyPreset.Custom,
            ["HVAC Controls", "Safety", "Other"]);

        var stored = converter.ConvertToProvider(policy);
        var restored = Assert.IsType<SiteMediaPolicy>(converter.ConvertFromProvider(stored));

        Assert.Equal(policy, restored);
        Assert.Equal(MediaPolicyPreset.Custom, restored.Preset);
        Assert.Equal(["HVAC Controls", "Safety", "Other"], restored.Categories);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=SiteWatchModelMetadata;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, Substitute.For<IMediator>());
    }
}
