using Application.SeedWork.Interfaces;
using Application.Sites.Commands;
using Application.Sites.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Ardalis.GuardClauses;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Sites.Services;

public sealed class SiteService(ApplicationDbContext dbContext, IMapper mapper) : ISiteService
{
    public async Task<Guid> CreateAsync(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        if (!SiteMediaPolicy.TryParsePreset(request.MediaPolicyPreset, out var preset))
        {
            throw new ArgumentException("Unsupported media policy preset.", nameof(request.MediaPolicyPreset));
        }

        var managerExists = await dbContext.Users
            .AnyAsync(user => user.Id == request.ManagerId, cancellationToken);

        if (!managerExists)
        {
            throw new ArgumentException("Site manager was not found.", nameof(request.ManagerId));
        }

        var status = ParseStatus(request.Status);
        var startDate = ParseDate(request.StartDate, nameof(request.StartDate));
        var endDate = ParseOptionalDate(request.EndDate, nameof(request.EndDate));
        var mediaPolicy = SiteMediaPolicy.Create(preset, request.MediaCategories);
        var site = new Site(
            request.Name,
            request.Address,
            request.ManagerId,
            startDate,
            status,
            endDate,
            mediaPolicy);

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
            .Include(site => site.Manager)
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

        var managerExists = await dbContext.Users
            .AnyAsync(user => user.Id == request.ManagerId, cancellationToken);

        if (!managerExists)
        {
            throw new ArgumentException("Site manager was not found.", nameof(request.ManagerId));
        }

        site.UpdateDetails(
            request.Name,
            request.Address,
            request.ManagerId,
            ParseDate(request.StartDate, nameof(request.StartDate)),
            ParseOptionalDate(request.EndDate, nameof(request.EndDate)),
            ParseStatus(request.Status),
            preset);
        site.MediaPolicy.AddCategories(request.MediaCategoriesToAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SiteStatus ParseStatus(string value)
    {
        if (Enum.TryParse<SiteStatus>(value, true, out var status)
            && Enum.IsDefined(typeof(SiteStatus), status))
        {
            return status;
        }

        throw new ArgumentException("Unsupported site status.", nameof(value));
    }

    private static DateOnly ParseDate(string value, string parameterName)
    {
        if (DateOnly.TryParse(value, out var date))
        {
            return date;
        }

        throw new ArgumentException("Invalid date.", parameterName);
    }

    private static DateOnly? ParseOptionalDate(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseDate(value, parameterName);
}
