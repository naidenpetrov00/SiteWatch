using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.SeedData;

public sealed class Atelier48VSiteSeedData(
    ApplicationDbContext dbContext,
    ILogger<Atelier48VSiteSeedData> logger)
{
    private const string SeededBy = "System";

    public const string SiteName = "Ателие 48В";
    public const string SiteAddress = "ул. Демонстрационна 48В, София";

    public static readonly IReadOnlyList<string> RoomCategories =
    [
        "Антре",
        "Баня",
        "Кухня",
        "Спалня",
        "Хол",
    ];

    public async Task<Site?> SeedAsync(
        IReadOnlyCollection<ApplicationUser> users,
        CancellationToken cancellationToken = default)
    {
        var manager = FindUser(users, ApplicationUserSeedData.AdministratorEmail);
        var client = FindUser(users, ApplicationUserSeedData.ClientEmail);
        if (manager is null || client is null)
        {
            logger.LogWarning(
                "Site {SiteName} was not seeded because manager {ManagerEmail} or client {ClientEmail} is missing.",
                SiteName,
                ApplicationUserSeedData.AdministratorEmail,
                ApplicationUserSeedData.ClientEmail);
            return null;
        }

        var candidates = await dbContext.Sites
            .Include(site => site.Users)
            .Where(site =>
                site.Name.Value == SiteName
                || site.Address.Value == SiteAddress)
            .ToListAsync(cancellationToken);
        var namedSites = candidates
            .Where(site => site.Name.Value == SiteName)
            .ToArray();

        if (namedSites.Length > 1)
        {
            logger.LogWarning(
                "Site {SiteName} was not seeded because multiple records use its name.",
                SiteName);
            return null;
        }

        var site = namedSites.SingleOrDefault();
        var addressIsUsedByAnotherSite = candidates.Any(candidate =>
            candidate.Name.Value != SiteName
            && candidate.Address.Value == SiteAddress);
        if (addressIsUsedByAnotherSite)
        {
            logger.LogWarning(
                "Site {SiteName} was not seeded because its development address is already used by another site.",
                SiteName);
            return null;
        }

        if (site is not null
            && site.Address.Value != SiteAddress
            && !string.Equals(site.CreatedBy, SeededBy, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Site {SiteName} was not seeded because a non-seed record already uses that name with a different address.",
                SiteName);
            return null;
        }

        var requiredMediaPolicy = SiteMediaPolicy.Create(
            MediaPolicyPreset.Custom,
            RoomCategories);
        var created = site is null;

        if (site is null)
        {
            site = new Site(
                SiteName,
                SiteAddress,
                manager.Id,
                new DateOnly(2026, 10, 1),
                mediaPolicy: requiredMediaPolicy);
            SetAuditValues(site, created: true);
            site.AddUser(manager);
            site.AssignPrimaryClientRecipient(client);
            dbContext.Sites.Add(site);
        }
        else
        {
            if (!string.Equals(site.ManagerId, manager.Id, StringComparison.Ordinal)
                || site.Address.Value != SiteAddress)
            {
                site.UpdateDetails(
                    site.Name.Value,
                    SiteAddress,
                    manager.Id,
                    site.StartDate,
                    site.EndDate,
                    site.Status,
                    site.MediaPolicy.Preset);
            }

            if (!site.MediaPolicy.Equals(requiredMediaPolicy))
            {
                site.ChangeMediaPolicy(requiredMediaPolicy);
            }

            site.AddUser(manager);
            site.AssignPrimaryClientRecipient(client);
            if (dbContext.ChangeTracker.HasChanges())
            {
                SetAuditValues(site, created: false);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (created)
        {
            logger.LogInformation(
                "Seeded site {SiteName} with manager {ManagerEmail} and client {ClientEmail}.",
                SiteName,
                ApplicationUserSeedData.AdministratorEmail,
                ApplicationUserSeedData.ClientEmail);
        }
        else
        {
            logger.LogInformation(
                "Ensured site {SiteName}, its media classifications, manager and client access.",
                SiteName);
        }

        return site;
    }

    private static ApplicationUser? FindUser(
        IEnumerable<ApplicationUser> users,
        string email) => users.SingleOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));

    private static void SetAuditValues(Site site, bool created)
    {
        var now = DateTimeOffset.UtcNow;
        if (created)
        {
            site.Created = now;
            site.CreatedBy = SeededBy;
        }

        site.LastModified = now;
        site.LastModifiedBy = SeededBy;
    }
}
