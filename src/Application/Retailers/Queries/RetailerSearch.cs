using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Retailers.Queries;

/// <summary>Searches active retailers for future listing and offer assignment.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record RetailerSearchQuery : IRequest<List<RetailerLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class RetailerSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RetailerSearchQuery, List<RetailerLookupDto>>
{
    public async Task<List<RetailerLookupDto>> Handle(
        RetailerSearchQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeNameSearch(request.SearchTerm);
        var normalizedHost = NormalizeHostSearch(request.SearchTerm);
        if (normalizedName.Length == 0 && normalizedHost.Length == 0)
        {
            return [];
        }

        var retailers = await dbContext.Retailers
            .AsNoTracking()
            .Where(retailer => retailer.IsActive
                && ((!string.IsNullOrEmpty(normalizedName)
                        && retailer.NormalizedName.Contains(normalizedName))
                    || (!string.IsNullOrEmpty(normalizedHost)
                        && retailer.NormalizedWebsiteHost.Contains(normalizedHost))))
            .OrderBy(retailer => retailer.DisplayName)
            .ThenBy(retailer => retailer.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        return retailers.Select(RetailerLookupDto.From).ToList();
    }

    private static string NormalizeNameSearch(string? value) =>
        string.Join(
            " ",
            (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        .ToUpperInvariant();

    private static string NormalizeHostSearch(string? value)
    {
        var candidate = (value ?? string.Empty).Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            candidate = uri.IdnHost;
        }

        var normalized = candidate.ToLowerInvariant();
        if (normalized.StartsWith("https://", StringComparison.Ordinal))
        {
            normalized = normalized[8..];
        }
        else if (normalized.StartsWith("http://", StringComparison.Ordinal))
        {
            normalized = normalized[7..];
        }

        normalized = normalized.TrimEnd('/').TrimEnd('.');
        return normalized.StartsWith("www.", StringComparison.Ordinal) && normalized.Length > 4
            ? normalized[4..]
            : normalized;
    }
}

public sealed class RetailerSearchQueryValidator : AbstractValidator<RetailerSearchQuery>
{
    public RetailerSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
    }
}
