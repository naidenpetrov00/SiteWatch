using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Persons.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record PersonSearchQuery : IRequest<List<PersonLookupDto>>
{
    public string? SearchTerm { get; init; }
}

public sealed class PersonSearchQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<PersonSearchQuery, List<PersonLookupDto>>
{
    public async Task<List<PersonLookupDto>> Handle(
        PersonSearchQuery request,
        CancellationToken cancellationToken
    )
    {
        var normalizedSearchTerm = NormalizeSearchTerm(request.SearchTerm);
        var normalizedDigits = NormalizeDigits(request.SearchTerm);

        if (normalizedSearchTerm.Length == 0 && normalizedDigits.Length == 0)
        {
            return [];
        }

        var query = dbContext.Persons
            .AsNoTracking()
            .Where(person =>
                (!string.IsNullOrEmpty(normalizedSearchTerm) && person.SearchName.Contains(normalizedSearchTerm))
                || (!string.IsNullOrEmpty(normalizedDigits)
                    && person.SearchTaxIdentifier.Contains(normalizedDigits))
            )
            .OrderBy(person => person.SearchName)
            .ThenBy(person => person.Id)
            .Take(20);

        var persons = await query.ToListAsync(cancellationToken);
        return persons.Select(PersonLookupDto.From).ToList();
    }

    private static string NormalizeSearchTerm(string? value) =>
        string.Join(
            " ",
            (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );

    private static string NormalizeDigits(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());
}

public sealed class PersonSearchQueryValidator : AbstractValidator<PersonSearchQuery>
{
    public PersonSearchQueryValidator()
    {
        RuleFor(query => query.SearchTerm)
            .MaximumLength(200)
            .WithMessage("SearchTerm must be at most 200 characters.");
    }
}
