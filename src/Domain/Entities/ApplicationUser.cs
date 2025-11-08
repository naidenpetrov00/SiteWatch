using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    private readonly HashSet<Site> _sites = [];

    public virtual ICollection<Site> Sites => _sites;

    public void AddSite(Site site)
    {
        _sites.Add(site);
    }

    public void AddSiteRange(List<Site> sites)
    {
        foreach (var site in sites)
        {
            _sites.Add(site);
        }
    }
}
