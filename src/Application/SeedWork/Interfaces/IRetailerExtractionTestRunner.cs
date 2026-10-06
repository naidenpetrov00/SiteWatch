using Application.RetailerExtractionProfiles;
using Domain.Entities;

namespace Application.SeedWork.Interfaces;

public interface IRetailerExtractionEngine
{
    Task<RetailerExtractionEngineResult> RunAsync(
        string productUrl,
        IReadOnlySet<string> allowedHosts,
        IReadOnlyList<RetailerExtractionRule> rules,
        CancellationToken cancellationToken);
}

public sealed record RetailerExtractionEngineResult(
    DateTimeOffset ExtractedAt,
    string FinalUrl,
    RetailerExtractionEngineMatch? Match,
    IReadOnlyList<RetailerExtractionRuleDiagnosticDto> Diagnostics);

public sealed record RetailerExtractionEngineMatch(
    Guid RuleId,
    decimal Amount,
    string RawValue);
