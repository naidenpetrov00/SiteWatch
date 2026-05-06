
using Domain.SeedWork;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            .Where(entry => entry.Entity.Events.Count != 0);

        var entityEntries = entities as EntityEntry<BaseEntity>[] ?? entities.ToArray();
        var events = entityEntries.SelectMany(entity => entity.Entity.Events).ToList();

        entityEntries.ToList().ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in events)
            await mediator.Publish(domainEvent);
    }
}
