using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Site : BaseAuditableEntity
{
    private readonly HashSet<ApplicationUser> _users = [];

#pragma warning disable CS8618
    private Site() { }
#pragma warning restore CS8618

    public Site(SiteName name, SiteAddress address)
    {
        Name = name;
        Address = address;
    }

    public SiteName Name { get; private set; }
    public SiteAddress Address { get; private set; }
    public virtual IReadOnlyCollection<ApplicationUser> Users => _users;

    public void AddUser(ApplicationUser user) => _users.Add(user);

    public void AddUserRange(List<ApplicationUser> users)
    {
        foreach (var user in users)
        {
            _users.Add(user);
        }
    }
}
