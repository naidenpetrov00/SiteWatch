using Domain.Entities;

namespace Application.Persons.Queries;

public sealed record PersonLookupDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }

    public static PersonLookupDto From(Person person) =>
        new()
        {
            Id = person.Id,
            DisplayName = person.DisplayName,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            CompanyName = person.CompanyName
        };
}
