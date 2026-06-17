using Domain.Entities;

namespace Application.Persons.Queries;

public sealed record PersonTableDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? LegalForm { get; init; }
    public string? Egn { get; init; }
    public string? Eik { get; init; }
    public string VatNumber { get; init; } = string.Empty;

    public static PersonTableDto From(Person person) =>
        new()
        {
            Id = person.Id,
            Type = person.Type.ToString(),
            DisplayName = person.DisplayName,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            CompanyName = person.CompanyName,
            LegalForm = person.LegalForm?.ToString(),
            Egn = person.Egn,
            Eik = person.Eik,
            VatNumber = person.VatNumber
        };
}
