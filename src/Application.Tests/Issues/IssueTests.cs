using Application.Issues.Commands;
using Application.Issues.Queries;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Domain.Entities;
using Domain.SeedWork.Enums;
using NSubstitute;

namespace Application.Tests.Issues;

public sealed class IssueTests
{
    [Fact]
    public void Issue_creation_normalizes_required_text_and_defaults_to_open()
    {
        var issue = Issue.Create(Site(), "  Broken gate  ", "  The gate will not close.  ");

        Assert.Equal("Broken gate", issue.Title);
        Assert.Equal("The gate will not close.", issue.Description);
        Assert.Equal(IssueStatus.Open, issue.Status);
    }

    [Fact]
    public void Issue_rejects_an_end_date_before_its_start_date()
    {
        var issue = Issue.Create(Site(), "Broken gate", "The gate will not close.");

        var exception = Assert.Throws<ArgumentException>(() => issue.UpdateDetails(
            "Broken gate", "The gate will not close.", IssueStatus.Open,
            new DateOnly(2026, 4, 2), new DateOnly(2026, 4, 1)));

        Assert.Equal("endDate", exception.ParamName);
    }

    [Fact]
    public void Issue_replaces_assignments_and_rejects_duplicate_worker_identifiers()
    {
        var issue = Issue.Create(Site(), "Broken gate", "The gate will not close.");
        var firstWorker = new ApplicationUser { Id = "worker-1" };
        var replacementWorker = new ApplicationUser { Id = "worker-2" };

        issue.ReplaceAssignedWorkers([firstWorker]);
        issue.ReplaceAssignedWorkers([replacementWorker]);

        Assert.Equal([replacementWorker], issue.AssignedWorkers);
        Assert.Throws<ArgumentException>(() => issue.ReplaceAssignedWorkers(
            [replacementWorker, new ApplicationUser { Id = "worker-2" }]));
    }

    [Fact]
    public async Task Issue_validators_enforce_the_public_upsert_contract()
    {
        var valid = ValidCreateCommand();
        var invalid = valid with
        {
            Title = new string('t', 201),
            Description = new string('d', 4001),
            AssignedWorkerIds = ["worker-1", "worker-1"],
            Status = "Unknown",
            StartDate = new DateOnly(2026, 4, 2),
            EndDate = new DateOnly(2026, 4, 1),
        };

        Assert.True((await new CreateIssueValidator().ValidateAsync(valid)).IsValid);
        var validation = await new CreateIssueValidator().ValidateAsync(invalid);
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(CreateIssueCommand.Title));
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(CreateIssueCommand.Description));
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(CreateIssueCommand.AssignedWorkerIds));
        Assert.Contains(validation.Errors, error => error.PropertyName == nameof(CreateIssueCommand.Status));
        Assert.Contains(validation.Errors, error => error.ErrorMessage == "End date cannot be before the start date.");

        var update = new UpdateIssueCommand { SiteId = valid.SiteId, Title = valid.Title, Description = valid.Description };
        var updateValidation = await new UpdateIssueValidator().ValidateAsync(update);
        Assert.Contains(updateValidation.Errors, error => error.PropertyName == nameof(UpdateIssueCommand.Id));
        Assert.Contains(updateValidation.Errors, error => error.PropertyName == nameof(UpdateIssueCommand.Status));
    }

    [Fact]
    public async Task Issue_handlers_forward_requests_and_cancellation_to_the_service()
    {
        var service = Substitute.For<IIssueService>();
        var token = new CancellationTokenSource().Token;
        var create = ValidCreateCommand();
        var update = new UpdateIssueCommand { Id = Guid.NewGuid(), SiteId = create.SiteId, Title = create.Title, Description = create.Description, Status = "Open" };
        var issue = IssueDetails(create.SiteId);
        service.CreateAsync(create, token).Returns(issue.Id);
        service.UpdateAsync(update, token).Returns(Task.CompletedTask);
        service.GetByIdAsync(issue.Id, token).Returns(issue);
        service.GetBySiteAsync(create.SiteId, token).Returns([issue]);
        var dashboard = new PagedResult<IssueDetailsDto>([issue], 1, 1);
        var dashboardQuery = new DashboardIssuesQuery { Status = "Open" };
        service.GetDashboardIssuesAsync(dashboardQuery, token).Returns(dashboard);

        Assert.Equal(issue.Id, await new CreateIssueHandler(service).Handle(create, token));
        await new UpdateIssueHandler(service).Handle(update, token);
        Assert.Same(issue, await new IssueByIdHandler(service).Handle(new IssueByIdQuery(issue.Id), token));
        Assert.Equal([issue], await new SiteIssuesHandler(service).Handle(new SiteIssuesQuery(create.SiteId), token));
        Assert.Same(dashboard, await new DashboardIssuesHandler(service).Handle(dashboardQuery, token));

        await service.Received(1).CreateAsync(create, token);
        await service.Received(1).UpdateAsync(update, token);
        await service.Received(1).GetByIdAsync(issue.Id, token);
        await service.Received(1).GetBySiteAsync(create.SiteId, token);
        await service.Received(1).GetDashboardIssuesAsync(dashboardQuery, token);
    }

    private static CreateIssueCommand ValidCreateCommand() => new()
    {
        SiteId = Guid.NewGuid(), Title = "Broken gate", Description = "The gate will not close.",
        Status = "Open", StartDate = new DateOnly(2026, 4, 1), EndDate = new DateOnly(2026, 4, 2),
        AssignedWorkerIds = ["worker-1"],
    };

    private static IssueDetailsDto IssueDetails(Guid siteId) => new(Guid.NewGuid(), 42, siteId, "North site", "Broken gate", "The gate will not close.", "Open", null, null, DateTimeOffset.UnixEpoch, "admin", DateTimeOffset.UnixEpoch, "admin", []);

    private static Site Site() => new("North site", "1 Main Street", "manager-1", new DateOnly(2026, 1, 1));
}
