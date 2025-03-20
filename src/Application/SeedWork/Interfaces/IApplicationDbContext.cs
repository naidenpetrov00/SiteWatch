namespace Application.SeedWork.Interfaces;

public interface IApplicationDbContext
{

    // DbSet<PackingItem> PackingItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
