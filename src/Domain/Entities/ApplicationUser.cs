using Microsoft.AspNetCore.Identity;
using Domain.SeedWork;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser, IHasNumberId
{
    private readonly HashSet<Site> _sites = [];
    private readonly HashSet<Site> _managedSites = [];

    public int NumberId { get; private set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public virtual ICollection<Site> Sites => _sites;
    public IReadOnlyCollection<Site> ManagedSites => _managedSites;

    public void AddSite(Site site) => _sites.Add(site);

    public void AddSiteRange(List<Site> sites)
    {
        foreach (var site in sites)
        {
            _sites.Add(site);
        }
    }
}
