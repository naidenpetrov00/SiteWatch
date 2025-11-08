
using Domain.SeedWork;
using Infrastructure.Data;
using MediatR;

namespace Infrastructure.SeedWork.Extension;
internal static class MediatorExtensions
{
    internal static async Task DispatchDomainEventsAsync(
        this IMediator mediator,
        ApplicationDbContext dbContext
    )
    {
        var entities = dbContext
            .ChangeTracker.Entries<BaseEntity>()
            .Where(entr => entr.Entity.Events is not null && entr.Entity.Events.Count != 0);

        var events = entities.SelectMany(entity => entity.Entity.Events).ToList();

        entities.ToList().ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in events)
            await mediator.Publish(domainEvent);
    }
}
