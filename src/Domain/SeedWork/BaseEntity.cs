using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.SeedWork;

public abstract class BaseEntity
{
#pragma warning disable CS8618
    public Guid Id { get; set; }
#pragma warning restore CS8618

    private List<BaseEvent> events = [];

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> Events => events.AsReadOnly();

    protected void AddDomainEvent(BaseEvent domainEvent)
    {
        events ??= [];
        events.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent) => events.Remove(domainEvent);

    public void ClearDomainEvents() => events.Clear();
}
