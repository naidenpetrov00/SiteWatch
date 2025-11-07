using Domain.SeedWork;

namespace Domain.Entities;

public class Site(string name) : BaseAuditableEntity
{
    public string Name { get; private set; } = name;
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
