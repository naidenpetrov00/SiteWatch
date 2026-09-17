using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Persons.Queries;

/// <summary>Represents an existing company Person available as a legal entity.</summary>
public sealed record CompanyPersonLookupDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? LegalForm { get; init; }
    public string Eik { get; init; } = string.Empty;
    public string VatNumber { get; init; } = string.Empty;

    public static CompanyPersonLookupDto From(Person person) =>
        new()
        {
            Id = person.Id,
            DisplayName = person.DisplayName,
            LegalForm = person.LegalForm?.ToString(),
            Eik = person.Eik ?? string.Empty,
            VatNumber = person.VatNumber
        };
}

/// <summary>Searches existing company Persons for retailer legal-entity selection.</summary>
[Authorize(Roles = UserRoles.Administrator)]
public sealed record CompanyPersonSearchQuery : IRequest<List<CompanyPersonLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class CompanyPersonSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CompanyPersonSearchQuery, List<CompanyPersonLookupDto>>
{
    public async Task<List<CompanyPersonLookupDto>> Handle(
        CompanyPersonSearchQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedSearchTerm = NormalizeSearchTerm(request.SearchTerm);
        var normalizedDigits = NormalizeDigits(request.SearchTerm);
        if (normalizedSearchTerm.Length == 0 && normalizedDigits.Length == 0)
        {
            return [];
        }

        var persons = await dbContext.Persons
            .AsNoTracking()
            .Where(person => person.Type == PersonType.Company
                && ((!string.IsNullOrEmpty(normalizedSearchTerm)
                        && (person.SearchName.Contains(normalizedSearchTerm)
                            || person.VatNumber.ToUpper().Contains(normalizedSearchTerm)))
                    || (!string.IsNullOrEmpty(normalizedDigits)
                        && person.SearchTaxIdentifier.Contains(normalizedDigits))))
            .OrderBy(person => person.SearchName)
            .ThenBy(person => person.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        return persons.Select(CompanyPersonLookupDto.From).ToList();
    }

    private static string NormalizeSearchTerm(string? value) =>
        string.Join(
            " ",
            (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string NormalizeDigits(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());
}

public sealed class CompanyPersonSearchQueryValidator
    : AbstractValidator<CompanyPersonSearchQuery>
{
    public CompanyPersonSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm).MaximumLength(200);
    }
}
