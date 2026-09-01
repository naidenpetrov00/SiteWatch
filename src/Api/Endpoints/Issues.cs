using Api.SeedWork;
using Api.SeedWork.Extensions;
using Application.Issues.Commands;
using Application.Issues.Queries;
using Application.SeedWork.Models;
using Application.SeedWork.Security;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public sealed class Issues : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroupCustom(customGroupName: "issues").RequireAuthorization();
        var dashboardGroup = app.MapGroupCustom(customGroupName: "dashboard").RequireAuthorization();

        group.MapPost(string.Empty, CreateIssue)
            .WithName("CreateIssue")
            .WithSummary("Create an issue")
            .Produces<IssueCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden);
        group.MapGet("/{issueId:guid}", GetIssue)
            .WithName("GetIssue")
            .WithSummary("Get an issue")
            .Produces<IssueDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        group.MapGet("/site/{siteId:guid}", GetSiteIssues)
            .WithName("GetSiteIssues")
            .WithSummary("Get issues for a site")
            .Produces<IReadOnlyList<IssueDetailsDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPut("/{issueId:guid}", UpdateIssue)
            .RequireAuthorization(AuthorizationPolicies.Administrator)
            .WithName("UpdateIssue")
            .WithSummary("Update an issue")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
        dashboardGroup.MapGet("/issues", GetDashboardIssues)
            .WithName("GetDashboardIssues")
            .WithSummary("Get issues for the dashboard table")
            .Produces<PagedResult<IssueDetailsDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<Created<IssueCreatedResponse>> CreateIssue(
        IMediator mediator,
        CreateIssueCommand command,
        CancellationToken cancellationToken)
    {
        var issueId = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/issues/{issueId}", new IssueCreatedResponse(issueId));
    }

    private static async Task<Ok<IssueDetailsDto>> GetIssue(
        IMediator mediator,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        var issue = await mediator.Send(new IssueByIdQuery(issueId), cancellationToken);
        return TypedResults.Ok(issue);
    }

    private static async Task<Ok<IReadOnlyList<IssueDetailsDto>>> GetSiteIssues(
        IMediator mediator,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var issues = await mediator.Send(new SiteIssuesQuery(siteId), cancellationToken);
        return TypedResults.Ok(issues);
    }

    private static async Task<NoContent> UpdateIssue(
        IMediator mediator,
        Guid issueId,
        UpdateIssueCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = issueId;
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<PagedResult<IssueDetailsDto>>> GetDashboardIssues(
        IMediator mediator,
        [AsParameters] DashboardIssuesQuery query,
        CancellationToken cancellationToken)
    {
        var issues = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(issues);
    }

    /// <summary>Represents the identifier of a newly created issue.</summary>
    public sealed record IssueCreatedResponse(Guid Id);
}
