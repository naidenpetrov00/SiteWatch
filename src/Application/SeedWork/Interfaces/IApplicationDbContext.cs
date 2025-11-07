using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.SeedWork.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Site> Sites { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
