using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sites.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record SiteSearchQuery : IRequest<List<SiteLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class SiteSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SiteSearchQuery, List<SiteLookupDto>>
{
    private const int DefaultSuggestionsPerClassification = 5;

    public async Task<List<SiteLookupDto>> Handle(
        SiteSearchQuery request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm?.Trim() ?? string.Empty;
        if (searchTerm.Length == 0)
        {
            var companyStockSites = await dbContext.Sites
                .AsNoTracking()
                .Where(site => site.IsCompanyStock)
                .OrderBy(site => site.Name.Value)
                .ThenBy(site => site.NumberId)
                .ThenBy(site => site.Id)
                .Take(DefaultSuggestionsPerClassification)
                .ToListAsync(cancellationToken);
            var ordinarySites = await dbContext.Sites
                .AsNoTracking()
                .Where(site => !site.IsCompanyStock)
                .OrderBy(site => site.Name.Value)
                .ThenBy(site => site.NumberId)
                .ThenBy(site => site.Id)
                .Take(DefaultSuggestionsPerClassification)
                .ToListAsync(cancellationToken);

            return companyStockSites
                .Concat(ordinarySites)
                .Select(SiteLookupDto.From)
                .ToList();
        }

        var hasNumberId = int.TryParse(searchTerm, out var numberId);
        var sites = await dbContext.Sites
            .AsNoTracking()
            .Where(site =>
                site.Name.Value.Contains(searchTerm)
                || site.Address.Value.Contains(searchTerm)
                || (hasNumberId && site.NumberId == numberId))
            .OrderBy(site => site.Name.Value)
            .ThenBy(site => site.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        return sites.Select(SiteLookupDto.From).ToList();
    }
}

public sealed record SiteLookupDto(
    Guid Id,
    int NumberId,
    string Name,
    string Address,
    bool IsCompanyStock)
{
    public static SiteLookupDto From(Site site) =>
        new(
            site.Id,
            site.NumberId,
            site.Name.Value,
            site.Address.Value,
            site.IsCompanyStock);
}

public sealed class SiteSearchQueryValidator : AbstractValidator<SiteSearchQuery>
{
    public SiteSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm)
            .MaximumLength(200)
            .WithMessage("SearchTerm must be at most 200 characters.");
    }
}
