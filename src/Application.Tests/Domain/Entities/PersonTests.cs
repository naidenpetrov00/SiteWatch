using Domain.Entities;
using Xunit;

namespace Application.Tests.Domain.Entities;

public class PersonTests
{
    [Fact]
    public void CreateIndividual_TrimsInputAndBuildsNormalizedDisplayName()
    {
        var person = Person.CreateIndividual("  Anna  ", "  Ivanova ", "1234567890", "  Maria ");

        Assert.Equal("Anna", person.FirstName);
        Assert.Equal("Maria", person.MiddleName);
        Assert.Equal("Ivanova", person.LastName);
        Assert.Equal("Anna Maria Ivanova", person.DisplayName);
        Assert.Equal("1234567890", person.Egn);
        Assert.Equal("1234567890", person.SearchTaxIdentifier);
        Assert.Equal("1234567890", person.VatNumber);
    }

    [Fact]
    public void CreateCompany_TrimsInputAndBuildsNormalizedDisplayName()
    {
        var person = Person.CreateCompany("  Acme Ltd  ", "BG123456789");

        Assert.Equal("Acme Ltd", person.CompanyName);
        Assert.Equal("Acme Ltd", person.DisplayName);
        Assert.Equal("123456789", person.SearchTaxIdentifier);
        Assert.Equal("BG123456789", person.VatNumber);
    }

    [Fact]
    public void UpdateIndividual_InvalidEgn_DoesNotMutateExistingState()
    {
        var person = Person.CreateIndividual("Anna", "Ivanova", "1234567890", "Maria");

        var originalType = person.Type;
        var originalFirstName = person.FirstName;
        var originalMiddleName = person.MiddleName;
        var originalLastName = person.LastName;
        var originalCompanyName = person.CompanyName;
        var originalEgn = person.Egn;
        var originalEik = person.Eik;
        var originalVatNumber = person.VatNumber;
        var originalSearchName = person.SearchName;
        var originalSearchTaxIdentifier = person.SearchTaxIdentifier;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => person.UpdateIndividual("Changed", "Person", "12345", "Updated"));

        Assert.Equal(originalType, person.Type);
        Assert.Equal(originalFirstName, person.FirstName);
        Assert.Equal(originalMiddleName, person.MiddleName);
        Assert.Equal(originalLastName, person.LastName);
        Assert.Equal(originalCompanyName, person.CompanyName);
        Assert.Equal(originalEgn, person.Egn);
        Assert.Equal(originalEik, person.Eik);
        Assert.Equal(originalVatNumber, person.VatNumber);
        Assert.Equal(originalSearchName, person.SearchName);
        Assert.Equal(originalSearchTaxIdentifier, person.SearchTaxIdentifier);
    }

    [Fact]
    public void UpdateCompany_InvalidEik_DoesNotMutateExistingState()
    {
        var person = Person.CreateCompany("Acme Ltd", "BG123456789");

        var originalType = person.Type;
        var originalFirstName = person.FirstName;
        var originalMiddleName = person.MiddleName;
        var originalLastName = person.LastName;
        var originalCompanyName = person.CompanyName;
        var originalEgn = person.Egn;
        var originalEik = person.Eik;
        var originalVatNumber = person.VatNumber;
        var originalSearchName = person.SearchName;
        var originalSearchTaxIdentifier = person.SearchTaxIdentifier;

        Assert.Throws<ArgumentOutOfRangeException>(() => person.UpdateCompany("Changed Ltd", "12"));

        Assert.Equal(originalType, person.Type);
        Assert.Equal(originalFirstName, person.FirstName);
        Assert.Equal(originalMiddleName, person.MiddleName);
        Assert.Equal(originalLastName, person.LastName);
        Assert.Equal(originalCompanyName, person.CompanyName);
        Assert.Equal(originalEgn, person.Egn);
        Assert.Equal(originalEik, person.Eik);
        Assert.Equal(originalVatNumber, person.VatNumber);
        Assert.Equal(originalSearchName, person.SearchName);
        Assert.Equal(originalSearchTaxIdentifier, person.SearchTaxIdentifier);
    }
}
