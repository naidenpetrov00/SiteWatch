using System.Reflection;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Infrastructure.SeedWork.Extension;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Camera> Cameras => Set<Camera>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await mediator.DispatchDomainEventsAsync(this);

        return await base.SaveChangesAsync(cancellationToken);
    }
}