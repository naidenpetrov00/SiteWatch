using Ardalis.GuardClauses;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Retailer : BaseAuditableEntity, IAgregateRoot
{
    public const int MaxDisplayNameLength = 200;
    public const int MaxNormalizedNameLength = 400;
    public const int MaxNotesLength = 2000;

    private Retailer()
    {
    }

    public string DisplayName { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public Guid CompanyPersonId { get; private set; }
    public Person CompanyPerson { get; private set; } = null!;
    public string BaseWebsiteUrl { get; private set; } = null!;
    public string NormalizedWebsiteHost { get; private set; } = null!;
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    public static Retailer Create(
        string displayName,
        Person companyPerson,
        string baseWebsiteUrl,
        string? notes)
    {
        var retailer = new Retailer
        {
            Id = Guid.NewGuid(),
            IsActive = true
        };
        retailer.UpdateDetails(displayName, companyPerson, baseWebsiteUrl, notes);
        return retailer;
    }

    public void UpdateDetails(
        string displayName,
        Person companyPerson,
        string baseWebsiteUrl,
        string? notes)
    {
        var normalizedDisplayName = NormalizeDisplayName(displayName);
        var website = RetailerWebsite.Create(baseWebsiteUrl);

        SetCompanyPerson(companyPerson);
        DisplayName = normalizedDisplayName;
        NormalizedName = NormalizeNameKey(normalizedDisplayName);
        BaseWebsiteUrl = website.BaseUrl;
        NormalizedWebsiteHost = website.NormalizedHost;
        Notes = NormalizeNotes(notes);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public static string NormalizeNameKey(string value)
    {
        var normalized = NormalizeDisplayName(value).ToUpperInvariant();
        if (normalized.Length > MaxNormalizedNameLength)
        {
            throw new ArgumentException(
                $"The normalized retailer display name must be at most {MaxNormalizedNameLength} characters.",
                nameof(value));
        }

        return normalized;
    }

    public static bool IsMeaningfulDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = CollapseWhitespace(value);
        return normalized.Length <= MaxDisplayNameLength
            && normalized.Any(char.IsLetterOrDigit);
    }

    private void SetCompanyPerson(Person companyPerson)
    {
        var person = Guard.Against.Null(companyPerson);
        if (person.Type != PersonType.Company)
        {
            throw new ArgumentException(
                "A retailer legal entity must be a company Person.",
                nameof(companyPerson));
        }

        CompanyPerson = person;
        CompanyPersonId = person.Id;
    }

    private static string NormalizeDisplayName(string value)
    {
        var normalized = CollapseWhitespace(Guard.Against.NullOrWhiteSpace(value));
        if (normalized.Length > MaxDisplayNameLength || !normalized.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException(
                $"A retailer display name must contain a letter or digit and be at most {MaxDisplayNameLength} characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeNotes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxNotesLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                normalized.Length,
                $"Retailer notes must be at most {MaxNotesLength} characters.");
        }

        return normalized;
    }

    private static string CollapseWhitespace(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
