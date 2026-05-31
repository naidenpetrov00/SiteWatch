using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.SeedWork.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Site> Sites { get; }
    DbSet<SiteImage> SiteImages { get; }
    DbSet<SiteFile> SiteFiles { get; }
    DbSet<SiteVideo> SiteVideos { get; }
    DbSet<InvoiceDocument> InvoiceDocuments { get; }
    DbSet<Camera> Cameras { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
