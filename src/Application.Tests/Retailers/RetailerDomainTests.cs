using Application.Retailers.Queries;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Tests.Retailers;

public sealed class RetailerDomainTests
{
    [Fact]
    public void Create_normalizes_details_and_canonicalizes_the_website_origin()
    {
        var retailer = Retailer.Create(
            "  Example   Store  ", Company(), " HTTPS://WWW.Example.COM:8443/ ", "  Notes  ");

        Assert.Equal("Example Store", retailer.DisplayName);
        Assert.Equal("EXAMPLE STORE", retailer.NormalizedName);
        Assert.Equal("https://www.example.com:8443", retailer.BaseWebsiteUrl);
        Assert.Equal("example.com", retailer.NormalizedWebsiteHost);
        Assert.Equal("Notes", retailer.Notes);
        Assert.True(retailer.IsActive);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("https://example.com/catalog")]
    [InlineData("https://user:password@example.com")]
    [InlineData("https://example.com/?page=1")]
    public void Website_rejects_non_origin_or_non_http_values(string value) =>
        Assert.False(RetailerWebsite.TryCreate(value, out _));

    [Fact]
    public void Create_rejects_an_individual_as_the_legal_company() =>
        Assert.Throws<ArgumentException>(() => Retailer.Create(
            "Example Store",
            Person.CreateIndividual("Ada", "Lovelace", "1234567890", "BG123"),
            "https://example.com",
            null));

    [Theory]
    [InlineData("   ")]
    [InlineData("---")]
    public void Create_rejects_a_display_name_without_letters_or_digits(string displayName) =>
        Assert.Throws<ArgumentException>(() => Retailer.Create(
            displayName, Company(), "https://example.com", null));

    [Fact]
    public void Update_rejects_notes_over_the_supported_length() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Retailer.Create(
            "Example Store", Company(), "https://example.com", new string('x', Retailer.MaxNotesLength + 1)));

    [Fact]
    public void Retailer_can_be_deactivated_and_reactivated_without_losing_its_identity()
    {
        var retailer = Retailer.Create("Example Store", Company(), "https://example.com", null);
        var id = retailer.Id;

        retailer.Deactivate();
        retailer.Activate();

        Assert.Equal(id, retailer.Id);
        Assert.True(retailer.IsActive);
    }

    [Fact]
    public void Lookup_dto_exposes_the_canonical_storefront_contract()
    {
        var retailer = Retailer.Create("Example Store", Company(), "https://www.example.com", null);

        var dto = RetailerLookupDto.From(retailer);

        Assert.Equal(retailer.Id, dto.Id);
        Assert.Equal("https://www.example.com", dto.BaseWebsiteUrl);
        Assert.Equal("example.com", dto.WebsiteHost);
    }

    private static Person Company() => Person.CreateCompany("Example Ltd", null, "123456789", "BG123");
}
