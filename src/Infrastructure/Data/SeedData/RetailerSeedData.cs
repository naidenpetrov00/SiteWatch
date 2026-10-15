using Domain.Entities;
using Domain.SeedWork.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class RetailerSeedData(
    ApplicationDbContext dbContext,
    ILogger<RetailerSeedData> logger)
{
    private const string SeededBy = "System";

    public async Task SeedAsync(IReadOnlyCollection<Person> persons)
    {
        var definitions = new[]
        {
            new RetailerDefinition(
                "Praktiker",
                PersonSeedData.PraktikerEik,
                "https://praktiker.bg/"),
            new RetailerDefinition(
                "Maxxmart",
                PersonSeedData.MaxxmartEik,
                "https://www.maxxmart.eu/"),
        };
        var addedCount = 0;

        foreach (var definition in definitions)
        {
            var normalizedName = Retailer.NormalizeNameKey(definition.DisplayName);
            if (await dbContext.Retailers.AnyAsync(retailer =>
                retailer.NormalizedName == normalizedName))
            {
                continue;
            }

            var companyPerson = persons.Single(person =>
                person.Type == PersonType.Company
                && person.Eik == definition.CompanyEik);
            var retailer = Retailer.Create(
                definition.DisplayName,
                companyPerson,
                definition.BaseWebsiteUrl,
                notes: null);
            ApplyAuditMetadata(retailer);
            dbContext.Retailers.Add(retailer);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        logger.LogInformation(
            "Ensured {RetailerCount} demo retailers; created {CreatedRetailerCount}.",
            definitions.Length,
            addedCount);
    }

    private static void ApplyAuditMetadata(Retailer retailer)
    {
        var now = DateTimeOffset.UtcNow;
        retailer.Created = now;
        retailer.CreatedBy = SeededBy;
        retailer.LastModified = now;
        retailer.LastModifiedBy = SeededBy;
    }

    private sealed record RetailerDefinition(
        string DisplayName,
        string CompanyEik,
        string BaseWebsiteUrl);
}
