namespace Domain.SeedWork;

using System.ComponentModel.DataAnnotations.Schema;

public abstract class BaseEntity
{
#pragma warning disable CS8618
    public int Id { get; private set; }
#pragma warning restore CS8618

    private List<BaseEvent> events = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> Events => events.AsReadOnly();

    protected void AddDomainEvent(BaseEvent domainEvent)
    {
        events ??= new List<BaseEvent>();
        events.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent) => events.Remove(domainEvent);

    public void ClearDomainEvents() => events.Clear();
}
