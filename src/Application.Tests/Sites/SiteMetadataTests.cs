using Application.Sites.Commands;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using NSubstitute;

namespace Application.Tests.Sites;

public sealed class SiteMetadataTests
{
    [Fact]
    public void Site_initializes_manager_schedule_and_the_default_planning_status()
    {
        var startDate = new DateOnly(2026, 3, 3);
        var site = CreateSite(startDate, endDate: startDate);

        Assert.Equal("manager-1", site.ManagerId);
        Assert.Equal(startDate, site.StartDate);
        Assert.Equal(startDate, site.EndDate);
        Assert.Equal(SiteStatus.Planning, site.Status);
    }

    [Fact]
    public void Site_updates_its_manager_schedule_status_and_media_policy()
    {
        var site = CreateSite(new DateOnly(2026, 3, 3));

        site.UpdateDetails(
            "Updated site",
            "456 Updated Street",
            "manager-2",
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30),
            SiteStatus.Operational,
            MediaPolicyPreset.Custom);

        Assert.Equal("manager-2", site.ManagerId);
        Assert.Equal(new DateOnly(2026, 4, 1), site.StartDate);
        Assert.Equal(new DateOnly(2026, 4, 30), site.EndDate);
        Assert.Equal(SiteStatus.Operational, site.Status);
        Assert.Equal(MediaPolicyPreset.Custom, site.MediaPolicy.Preset);
    }

    [Theory]
    [InlineData(2026, 3, 2)]
    [InlineData(2026, 2, 28)]
    public void Site_rejects_an_end_date_before_its_start_date(int year, int month, int day)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateSite(new DateOnly(2026, 3, 3), new DateOnly(year, month, day)));

        Assert.Equal("endDate", exception.ParamName);
    }

    [Fact]
    public void Site_rejects_a_blank_manager_identifier()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Site("North Gate", SiteAddress.Create("1420 Industrial Park"), " ", new DateOnly(2026, 3, 3)));

        Assert.Equal("managerId", exception.ParamName);
    }

    [Fact]
    public async Task Create_site_validator_accepts_case_insensitive_status_and_equal_dates()
    {
        var command = ValidCreateCommand() with { EndDate = "2026-03-03", Status = "operational" };

        var result = await new CreateSiteValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "2026-03-03", null, "Planning", nameof(CreateSiteCommand.ManagerId))]
    [InlineData("manager-1", "not-a-date", null, "Planning", nameof(CreateSiteCommand.StartDate))]
    [InlineData("manager-1", "2026-03-03", "not-a-date", "Planning", nameof(CreateSiteCommand.EndDate))]
    [InlineData("manager-1", "2026-03-03", "2026-03-02", "Planning", "")]
    [InlineData("manager-1", "2026-03-03", null, "Archived", nameof(CreateSiteCommand.Status))]
    public async Task Create_site_validator_rejects_invalid_site_metadata(
        string managerId,
        string startDate,
        string? endDate,
        string status,
        string propertyName)
    {
        var command = ValidCreateCommand() with
        {
            ManagerId = managerId, StartDate = startDate, EndDate = endDate, Status = status
        };

        var result = await new CreateSiteValidator().ValidateAsync(command);

        Assert.True(result.Errors.Any(error =>
            propertyName.Length == 0
                ? error.ErrorMessage == "EndDate cannot be before StartDate."
                : error.PropertyName == propertyName));
    }

    [Fact]
    public async Task Update_site_validator_rejects_an_end_date_before_the_start_date()
    {
        var command = new UpdateSiteCommand
        {
            Id = Guid.NewGuid(),
            Name = "North Gate",
            Address = "1420 Industrial Park",
            ManagerId = "manager-1",
            StartDate = "2026-03-03",
            EndDate = "2026-03-02",
            Status = "Planning",
            MediaPolicyPreset = "Custom",
            MediaCategoriesToAdd = null!,
        };

        var result = await new UpdateSiteValidator(Substitute.For<IApplicationDbContext>())
            .ValidateAsync(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage == "EndDate cannot be before StartDate.");
    }

    private static Site CreateSite(DateOnly startDate, DateOnly? endDate = null) =>
        new("North Gate", SiteAddress.Create("1420 Industrial Park"), "manager-1", startDate, endDate: endDate);

    private static CreateSiteCommand ValidCreateCommand() => new()
    {
        Name = "North Gate",
        Address = "1420 Industrial Park",
        ManagerId = "manager-1",
        StartDate = "2026-03-03",
        Status = "Planning",
        MediaPolicyPreset = "Regular"
    };
}
