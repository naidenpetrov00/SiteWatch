using Domain.SeedWork;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Site : BaseAuditableEntity
{
    private readonly HashSet<ApplicationUser> _users = [];

    public Site(SiteName name, SiteAddress address)
    {
        Name = name;
        Address = address;
    }

    // ReSharper disable once UnusedMember.Local
    private Site()
    {
    }

    public SiteName Name { get; private set; } = null!;
    public SiteAddress Address { get; private set; } = null!;
    public IReadOnlyCollection<ApplicationUser> Users => _users;
    public IReadOnlyCollection<Camera> Cameras { get; } = new List<Camera>();

    public void AddUser(ApplicationUser user) => _users.Add(user);

    public void AddUserRange(List<ApplicationUser> users)
    {
        foreach (var user in users)
        {
            _users.Add(user);
        }
    }
}