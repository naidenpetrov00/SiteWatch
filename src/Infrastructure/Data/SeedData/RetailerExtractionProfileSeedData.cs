using Domain.Entities;
using Domain.SeedWork;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class RetailerExtractionProfileSeedData(
    ApplicationDbContext dbContext,
    ILogger<RetailerExtractionProfileSeedData> logger)
{
    private const string SeededBy = "System";

    public async Task SeedAsync(IReadOnlyCollection<Person> persons)
    {
        var definitions = CreateDefinitions();
        var createdCount = 0;

        foreach (var definition in definitions)
        {
            var companyPerson = persons.SingleOrDefault(person =>
                person.Type == PersonType.Company
                && person.Eik == definition.CompanyEik);
            if (companyPerson is null)
            {
                logger.LogWarning(
                    "Extraction profile seeding for {RetailerName} was skipped because company Person {CompanyEik} is missing.",
                    definition.RetailerName,
                    definition.CompanyEik);
                continue;
            }

            if (await dbContext.RetailerExtractionProfiles.AnyAsync(profile =>
                profile.CompanyPersonId == companyPerson.Id))
            {
                continue;
            }

            var profile = RetailerExtractionProfile.CreateDraft(
                companyPerson,
                version: 1,
                initialHosts: definition.AllowedHosts);
            profile.AddRule(new RetailerExtractionRuleConfiguration(
                Name: $"{definition.RetailerName} product JSON-LD offer",
                IsEnabled: true,
                RuleType: RetailerExtractionRuleType.JsonLd,
                JsonLdObjectType: "Product",
                JsonLdPricePath: "offers.price",
                JsonLdCurrencyPath: "offers.priceCurrency",
                CssSelector: null,
                CssValueSource: null,
                CssAttributeName: null,
                DecimalSeparator: RetailerExtractionDecimalSeparator.Dot,
                ThousandsSeparator: RetailerExtractionThousandsSeparator.None,
                ExpectedCurrencyCode: RetailerExtractionRule.EuroCurrencyCode,
                PriceBasis: PriceBasis.Package,
                MinimumValue: 0.01m,
                MaximumValue: 100000m));
            ApplyAuditMetadata(profile);
            dbContext.RetailerExtractionProfiles.Add(profile);
            createdCount++;
        }

        if (createdCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation(
            "Ensured extraction profiles for {RetailerCount} demo retailers; created {CreatedProfileCount} draft profiles.",
            definitions.Count,
            createdCount);
    }

    private static List<ProfileDefinition> CreateDefinitions() =>
    [
        new(
            "Praktiker",
            PersonSeedData.PraktikerEik,
            ["praktiker.bg", "www.praktiker.bg"]),
        new(
            "Maxxmart",
            PersonSeedData.MaxxmartEik,
            ["www.maxxmart.eu"]),
    ];

    private static void ApplyAuditMetadata(BaseAuditableEntity entity)
    {
        var now = DateTimeOffset.UtcNow;
        entity.Created = now;
        entity.CreatedBy = SeededBy;
        entity.LastModified = now;
        entity.LastModifiedBy = SeededBy;
    }

    private sealed record ProfileDefinition(
        string RetailerName,
        string CompanyEik,
        IReadOnlyList<string> AllowedHosts);
}
