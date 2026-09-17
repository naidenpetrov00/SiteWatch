namespace Application.Retailers.Commands;

/// <summary>Contains the editable fields shared by retailer create and update operations.</summary>
public abstract record RetailerUpsertDto
{
    public string DisplayName { get; init; } = string.Empty;
    public Guid CompanyPersonId { get; init; }
    public string BaseWebsiteUrl { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
