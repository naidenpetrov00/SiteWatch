using Application.Persons.Queries;
using Domain.Entities;

namespace Application.Retailers.Queries;

/// <summary>Represents the complete editable details of a retailer.</summary>
public sealed record RetailerDetailsDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public CompanyPersonLookupDto CompanyPerson { get; init; } = new();
    public string BaseWebsiteUrl { get; init; } = string.Empty;
    public string WebsiteHost { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public bool IsActive { get; init; }

    public static RetailerDetailsDto From(Retailer retailer) =>
        new()
        {
            Id = retailer.Id,
            DisplayName = retailer.DisplayName,
            CompanyPerson = CompanyPersonLookupDto.From(retailer.CompanyPerson),
            BaseWebsiteUrl = retailer.BaseWebsiteUrl,
            WebsiteHost = retailer.NormalizedWebsiteHost,
            Notes = retailer.Notes,
            IsActive = retailer.IsActive
        };
}

/// <summary>Represents a retailer row in the administrative table.</summary>
public sealed record RetailerTableDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public Guid CompanyPersonId { get; init; }
    public string CompanyDisplayName { get; init; } = string.Empty;
    public string BaseWebsiteUrl { get; init; } = string.Empty;
    public string WebsiteHost { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    public static RetailerTableDto From(Retailer retailer) =>
        new()
        {
            Id = retailer.Id,
            DisplayName = retailer.DisplayName,
            CompanyPersonId = retailer.CompanyPersonId,
            CompanyDisplayName = retailer.CompanyPerson.DisplayName,
            BaseWebsiteUrl = retailer.BaseWebsiteUrl,
            WebsiteHost = retailer.NormalizedWebsiteHost,
            IsActive = retailer.IsActive
        };
}

/// <summary>Represents an active retailer available for future listing and offer assignment.</summary>
public sealed record RetailerLookupDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string BaseWebsiteUrl { get; init; } = string.Empty;
    public string WebsiteHost { get; init; } = string.Empty;

    public static RetailerLookupDto From(Retailer retailer) =>
        new()
        {
            Id = retailer.Id,
            DisplayName = retailer.DisplayName,
            BaseWebsiteUrl = retailer.BaseWebsiteUrl,
            WebsiteHost = retailer.NormalizedWebsiteHost
        };
}
