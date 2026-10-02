using Application.RetailerExtractionProfiles;
using Domain.Entities;

namespace Application.SeedWork.Interfaces;

public interface IRetailerExtractionTestRunner
{
    Task<RetailerExtractionRunnerResult> RunAsync(
        string productUrl,
        IReadOnlySet<string> allowedHosts,
        IReadOnlyList<RetailerExtractionRule> rules,
        CancellationToken cancellationToken);
}

public sealed record RetailerExtractionRunnerResult(
    DateTimeOffset TestedAt,
    string TestedUrl,
    RetailerExtractionRunnerMatch? Match,
    IReadOnlyList<RetailerExtractionRuleDiagnosticDto> Diagnostics);

public sealed record RetailerExtractionRunnerMatch(
    Guid RuleId,
    decimal Amount,
    string RawValue);
