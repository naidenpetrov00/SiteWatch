using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Site(SiteName name, SiteAddress address) : BaseAuditableEntity
{
    private readonly HashSet<ApplicationUser> _users = [];

    public SiteName Name { get; private set; } = name;
    public SiteAddress Address { get; private set; } = address;
    public virtual IReadOnlyCollection<ApplicationUser> Users => _users;

    public void AddUser(ApplicationUser user)
    {
        _users.Add(user);
    }

    public void AddUserRange(List<ApplicationUser> users)
    {
        foreach (var user in users)
        {
            _users.Add(user);
        }
    }
}
