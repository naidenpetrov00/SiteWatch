using Application.SeedWork.Interfaces;
using Application.Sites.Commands;
using Application.Sites.Queries;
using AutoMapper;
using Domain.SeedWork.Enums;
using Domain.Entities;
using Domain.ValueObjects;
using Ardalis.GuardClauses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public sealed class SiteService(ApplicationDbContext dbContext, IMapper mapper) : ISiteService
{
    public async Task<Guid> CreateAsync(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        if (
            !Enum.TryParse<MediaPolicyPreset>(request.MediaPolicyPreset, true, out var preset)
            || !Enum.IsDefined(typeof(MediaPolicyPreset), preset)
        )
        {
            throw new ArgumentException("Unsupported media policy preset.", nameof(request.MediaPolicyPreset));
        }

        var mediaPolicy = preset == MediaPolicyPreset.Regular
            ? SiteMediaPolicy.Regular()
            : SiteMediaPolicy.Custom([], []);
        var site = new Site(request.Name, request.Address, mediaPolicy);

        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync(cancellationToken);

        return site.Id;
    }

    public async Task<List<SitesDto>> GetSitesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var sites = await dbContext
            .Sites.AsNoTracking()
            .Where(site => site.Users.Any(user => user.Id == userId.ToString()))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<SitesDto>>(sites);
    }

    public async Task UpdateAsync(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await dbContext.Sites
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (site is null)
        {
            throw new NotFoundException(nameof(Site), request.Id.ToString());
        }

        if (
            !Enum.TryParse<MediaPolicyPreset>(request.MediaPolicyPreset, true, out var preset)
            || !Enum.IsDefined(typeof(MediaPolicyPreset), preset)
        )
        {
            throw new ArgumentException("Unsupported media policy preset.", nameof(request.MediaPolicyPreset));
        }

        site.UpdateDetails(request.Name, request.Address, preset);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
