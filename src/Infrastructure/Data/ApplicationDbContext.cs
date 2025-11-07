namespace Infrastructure.Data;

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.SeedWork.Interfaces;
using Domain.Entities;
using Infrastructure.SeedWork.Extension;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IMediator mediatR;

    public DbSet<Site> Sites => Set<Site>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediatR)
        : base(options)
    {
        this.mediatR = mediatR;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await this.mediatR.DispatchDomainEventsAsync(this);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
