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
    DbSet<ActivityCatalogNode> ActivityCatalogNodes { get; }
    DbSet<ActivityRequirementSection> ActivityRequirementSections { get; }
    DbSet<ActivityProductRequirement> ActivityProductRequirements { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<SitePayment> SitePayments { get; }
    DbSet<Issue> Issues { get; }
    DbSet<IssueAttachment> IssueAttachments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
