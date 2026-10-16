using System.Reflection;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Infrastructure.SeedWork.Extension;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<SiteImage> SiteImages => Set<SiteImage>();
    public DbSet<SiteFile> SiteFiles => Set<SiteFile>();
    public DbSet<SiteVideo> SiteVideos => Set<SiteVideo>();
    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Retailer> Retailers => Set<Retailer>();
    public DbSet<RetailerListing> RetailerListings => Set<RetailerListing>();
    public DbSet<RetailerPriceObservation> RetailerPriceObservations =>
        Set<RetailerPriceObservation>();
    public DbSet<RetailerExtractionProfile> RetailerExtractionProfiles =>
        Set<RetailerExtractionProfile>();
    public DbSet<RetailerExtractionAllowedHost> RetailerExtractionAllowedHosts =>
        Set<RetailerExtractionAllowedHost>();
    public DbSet<RetailerExtractionRule> RetailerExtractionRules =>
        Set<RetailerExtractionRule>();
    public DbSet<RetailerPriceCollectionRun> RetailerPriceCollectionRuns =>
        Set<RetailerPriceCollectionRun>();
    public DbSet<RetailerPriceCollectionRunItem> RetailerPriceCollectionRunItems =>
        Set<RetailerPriceCollectionRunItem>();
    internal DbSet<RetailerPriceCollectionWorkerLease> RetailerPriceCollectionWorkerLeases =>
        Set<RetailerPriceCollectionWorkerLease>();
    public DbSet<ActivityCatalogNode> ActivityCatalogNodes => Set<ActivityCatalogNode>();
    public DbSet<ActivityRequirementSection> ActivityRequirementSections =>
        Set<ActivityRequirementSection>();
    public DbSet<ActivityProductRequirement> ActivityProductRequirements =>
        Set<ActivityProductRequirement>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<SitePayment> SitePayments => Set<SitePayment>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferActivity> OfferActivities => Set<OfferActivity>();
    public DbSet<OfferActivitySection> OfferActivitySections => Set<OfferActivitySection>();
    public DbSet<OfferProductContribution> OfferProductContributions =>
        Set<OfferProductContribution>();
    public DbSet<OfferProductLine> OfferProductLines => Set<OfferProductLine>();
    public DbSet<OfferRetailerComparison> OfferRetailerComparisons =>
        Set<OfferRetailerComparison>();
    public DbSet<OfferProductPriceSelection> OfferProductPriceSelections =>
        Set<OfferProductPriceSelection>();
    public DbSet<AssetCatalogItem> AssetCatalogItems => Set<AssetCatalogItem>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetEvent> AssetEvents => Set<AssetEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddSequences();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureAssetHistoryIsAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await mediator.DispatchDomainEventsAsync(this);
        EnsureAssetHistoryIsAppendOnly();

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureAssetHistoryIsAppendOnly()
    {
        ChangeTracker.DetectChanges();

        if (ChangeTracker.Entries<Asset>().Any(entry => entry.State == EntityState.Deleted))
        {
            throw new InvalidOperationException("Assets cannot be hard-deleted.");
        }

        if (ChangeTracker.Entries<AssetEvent>().Any(entry =>
                entry.State is EntityState.Modified or EntityState.Deleted))
        {
            throw new InvalidOperationException(
                "Persisted asset history cannot be changed or deleted.");
        }
    }
}
