using Domain.Entities;
using Domain.SeedWork.Enums;
using Xunit;

namespace Application.Tests.Domain.Entities;

public class PersonTests
{
    [Fact]
    public void CreateIndividual_TrimsInputAndBuildsNormalizedDisplayName()
    {
        var person = Person.CreateIndividual(
            "  Anna  ",
            "  Ivanova ",
            "1234567890",
            "BG1234567890",
            "  Maria "
        );

        Assert.Equal("Anna", person.FirstName);
        Assert.Equal("Maria", person.MiddleName);
        Assert.Equal("Ivanova", person.LastName);
        Assert.Equal("Anna Maria Ivanova", person.DisplayName);
        Assert.Equal("1234567890", person.Egn);
        Assert.Equal("1234567890", person.SearchTaxIdentifier);
        Assert.Equal("BG1234567890", person.VatNumber);
    }

    [Fact]
    public void CreateCompany_TrimsInputAndBuildsNormalizedDisplayName()
    {
        var person = Person.CreateCompany(
            "  Смарт Проджектс  ",
            CompanyLegalForm.ООД,
            "BG123456789",
            "BG123456789"
        );

        Assert.Equal("Смарт Проджектс", person.CompanyName);
        Assert.Equal(CompanyLegalForm.ООД, person.LegalForm);
        Assert.Equal("Смарт Проджектс", person.DisplayName);
        Assert.Equal("СМАРТ ПРОДЖЕКТС", person.SearchName);
        Assert.Equal("123456789", person.SearchTaxIdentifier);
        Assert.Equal("BG123456789", person.VatNumber);
    }

    [Fact]
    public void UpdateIndividual_InvalidEgn_DoesNotMutateExistingState()
    {
        var person = Person.CreateIndividual(
            "Anna",
            "Ivanova",
            "1234567890",
            "BG1234567890",
            "Maria"
        );

        var originalType = person.Type;
        var originalFirstName = person.FirstName;
        var originalMiddleName = person.MiddleName;
        var originalLastName = person.LastName;
        var originalCompanyName = person.CompanyName;
        var originalLegalForm = person.LegalForm;
        var originalEgn = person.Egn;
        var originalEik = person.Eik;
        var originalVatNumber = person.VatNumber;
        var originalSearchName = person.SearchName;
        var originalSearchTaxIdentifier = person.SearchTaxIdentifier;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => person.UpdateIndividual("Changed", "Person", "12345", "BG1234567890", "Updated"));

        Assert.Equal(originalType, person.Type);
        Assert.Equal(originalFirstName, person.FirstName);
        Assert.Equal(originalMiddleName, person.MiddleName);
        Assert.Equal(originalLastName, person.LastName);
        Assert.Equal(originalCompanyName, person.CompanyName);
        Assert.Equal(originalLegalForm, person.LegalForm);
        Assert.Equal(originalEgn, person.Egn);
        Assert.Equal(originalEik, person.Eik);
        Assert.Equal(originalVatNumber, person.VatNumber);
        Assert.Equal(originalSearchName, person.SearchName);
        Assert.Equal(originalSearchTaxIdentifier, person.SearchTaxIdentifier);
    }

    [Fact]
    public void UpdateCompany_InvalidEik_DoesNotMutateExistingState()
    {
        var person = Person.CreateCompany(
            "Acme Ltd",
            CompanyLegalForm.ООД,
            "BG123456789",
            "BG123456789"
        );

        var originalType = person.Type;
        var originalFirstName = person.FirstName;
        var originalMiddleName = person.MiddleName;
        var originalLastName = person.LastName;
        var originalCompanyName = person.CompanyName;
        var originalLegalForm = person.LegalForm;
        var originalEgn = person.Egn;
        var originalEik = person.Eik;
        var originalVatNumber = person.VatNumber;
        var originalSearchName = person.SearchName;
        var originalSearchTaxIdentifier = person.SearchTaxIdentifier;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => person.UpdateCompany("Changed Ltd", CompanyLegalForm.АД, "12", "BG123456789"));

        Assert.Equal(originalType, person.Type);
        Assert.Equal(originalFirstName, person.FirstName);
        Assert.Equal(originalMiddleName, person.MiddleName);
        Assert.Equal(originalLastName, person.LastName);
        Assert.Equal(originalCompanyName, person.CompanyName);
        Assert.Equal(originalLegalForm, person.LegalForm);
        Assert.Equal(originalEgn, person.Egn);
        Assert.Equal(originalEik, person.Eik);
        Assert.Equal(originalVatNumber, person.VatNumber);
        Assert.Equal(originalSearchName, person.SearchName);
        Assert.Equal(originalSearchTaxIdentifier, person.SearchTaxIdentifier);
    }
}
