using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.SeedWork.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Site> Sites { get; }
    DbSet<SiteImage> SiteImages { get; }
    DbSet<SiteFile> SiteFiles { get; }
    DbSet<SiteVideo> SiteVideos { get; }
    DbSet<Camera> Cameras { get; }
    DbSet<Person> Persons { get; }
    DbSet<Product> Products { get; }
    DbSet<Retailer> Retailers { get; }
    DbSet<RetailerListing> RetailerListings { get; }
    DbSet<RetailerPriceObservation> RetailerPriceObservations { get; }
    DbSet<RetailerExtractionProfile> RetailerExtractionProfiles { get; }
    DbSet<RetailerExtractionAllowedHost> RetailerExtractionAllowedHosts { get; }
    DbSet<RetailerExtractionRule> RetailerExtractionRules { get; }
    DbSet<RetailerPriceCollectionRun> RetailerPriceCollectionRuns { get; }
    DbSet<RetailerPriceCollectionRunItem> RetailerPriceCollectionRunItems { get; }
    DbSet<ActivityCatalogNode> ActivityCatalogNodes { get; }
    DbSet<ActivityRequirementSection> ActivityRequirementSections { get; }
    DbSet<ActivityProductRequirement> ActivityProductRequirements { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<SitePayment> SitePayments { get; }
    DbSet<Issue> Issues { get; }
    DbSet<IssueAttachment> IssueAttachments { get; }
    DbSet<Offer> Offers { get; }
    DbSet<OfferActivity> OfferActivities { get; }
    DbSet<OfferActivitySection> OfferActivitySections { get; }
    DbSet<OfferProductContribution> OfferProductContributions { get; }
    DbSet<OfferProductLine> OfferProductLines { get; }
    DbSet<OfferRetailerComparison> OfferRetailerComparisons { get; }
    DbSet<OfferProductPriceSelection> OfferProductPriceSelections { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
