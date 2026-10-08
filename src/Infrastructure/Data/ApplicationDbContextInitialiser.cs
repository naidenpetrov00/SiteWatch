using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Data.SeedData;

namespace Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ApplicationUserSeedData applicationUserSeedData,
    PersonSeedData personSeedData,
    RetailerSeedData retailerSeedData,
    RenovationCatalogSeedData renovationCatalogSeedData,
    ActivityCatalogSeedData activityCatalogSeedData,
    RetailerExtractionProfileSeedData retailerExtractionProfileSeedData,
    BlobInitializer blobInitializer,
    ILogger<ApplicationDbContextInitialiser> logger
)
{
    private const string SeededBy = "System";
    private static readonly string[] InvoiceSeedSiteNames =
    [
        "Central Office",
        "Regional Office North",
        "Regional Office South",
    ];

    private static readonly string[] SeedUserEmails =
    [
        ApplicationUserSeedData.AdministratorEmail,
        "naidenpetrov00@gmail.com",
    ];

    private const string BulkSeedEmailDomain = "sitewatch.local";
    private async Task AddSites(List<ApplicationUser> users)
    {
        if (users.Count == 0 || await dbContext.Sites.AnyAsync())
            return;

        var administrator = users.Single(user =>
            string.Equals(
                user.Email,
                ApplicationUserSeedData.AdministratorEmail,
                StringComparison.OrdinalIgnoreCase));
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;
        var sites = new List<Site>
        {
            new(
                "Central Office",
                "Vitosha 17",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.SiteMaintenance)),
            new(
                "Vitosha Apartment Renovation",
                "Vitosha 17",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.ApartmentRenovation))
            {
                Created = now,
                CreatedBy = SeededBy,
                LastModified = now,
                LastModifiedBy = SeededBy,
            },
            new(
                "Regional Office North",
                "Dondukov 11",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.HouseBuild)),
            new(
                "Dondukov House Build",
                "Dondukov 11",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.HouseBuild))
            {
                Created = now,
                CreatedBy = SeededBy,
                LastModified = now,
                LastModifiedBy = SeededBy,
            },
            new(
                "Regional Office South",
                "Kestenova Gora 24",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.CommercialBuild)),
            new(
                "Kestenova Commercial Build",
                "Kestenova Gora 24",
                administrator.Id,
                startDate,
                mediaPolicy: SiteMediaPolicy.FromPreset(MediaPolicyPreset.CommercialBuild))
            {
                Created = now,
                CreatedBy = SeededBy,
                LastModified = now,
                LastModifiedBy = SeededBy,
            },
        };
        foreach (var site in sites)
        {
            site.AddUser(administrator);
        }
        sites[2].AddUserRange(users);

        await dbContext.Sites.AddRangeAsync(sites);
        await dbContext.SaveChangesAsync();
    }

    private async Task EnsureThirdSeedSiteAccessAsync(List<ApplicationUser> users)
    {
        var firstAdministrator = users.FirstOrDefault(user =>
            string.Equals(
                user.Email,
                ApplicationUserSeedData.AdministratorEmail,
                StringComparison.OrdinalIgnoreCase));
        if (firstAdministrator is null)
        {
            logger.LogWarning(
                "Third seeded site access was not added: seeded administrator {Email} was not found.",
                ApplicationUserSeedData.AdministratorEmail);
            return;
        }

        const string thirdSiteName = "Regional Office South";
        var thirdSite = await dbContext.Sites
            .Include(site => site.Users)
            .SingleOrDefaultAsync(site => site.Name.Value == thirdSiteName);
        if (thirdSite is null)
        {
            logger.LogWarning(
                "Third seeded site access was not added: site {SiteName} was not found.",
                thirdSiteName);
            return;
        }

        if (thirdSite.Users.Any(user => user.Id == firstAdministrator.Id))
        {
            return;
        }

        thirdSite.AddUser(firstAdministrator);
        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Assigned seeded administrator {Email} to site {SiteName}.",
            firstAdministrator.Email,
            thirdSiteName);
    }

    private async Task ClearSeedDataAsync()
    {
        var deletedIssues = await dbContext.Issues.ExecuteDeleteAsync();
        var deletedCameras = await dbContext.Cameras.ExecuteDeleteAsync();
        var deletedSites = await dbContext.Sites.ExecuteDeleteAsync();

        if (deletedIssues > 0 || deletedCameras > 0 || deletedSites > 0)
        {
            logger.LogInformation(
                "Cleared {IssueCount} issues, {CameraCount} cameras and {SiteCount} sites.",
                deletedIssues,
                deletedCameras,
                deletedSites
            );
        }

        foreach (var email in SeedUserEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                continue;

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                logger.LogInformation("Deleted seed user {Email}.", email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to delete seed user {Email}: {Errors}", email, errors);
            }
        }

        var bulkUsers = await userManager.Users
            .Where(user => user.Email != null && user.Email.EndsWith($"@{BulkSeedEmailDomain}"))
            .ToListAsync();

        foreach (var user in bulkUsers)
        {
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                logger.LogInformation("Deleted bulk seed user {Email}.", user.Email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning(
                    "Failed to delete bulk seed user {Email}: {Errors}",
                    user.Email,
                    errors
                );
            }
        }
    }

    private async Task AddCameras()
    {
        if (await dbContext.Cameras.AnyAsync())
        {
            logger.LogInformation("Camera seeding skipped: cameras already exist.");
            return;
        }

        var site = await dbContext.Sites
            .FirstOrDefaultAsync(site => site.Name.Value == "Regional Office North");

        if (site is null)
        {
            logger.LogWarning("Camera seeding skipped: site 'Regional Office North' not found.");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var cameras = new List<Camera>();
        for (var i = 1; i <= 5; i++)
        {
            var cameraBrand = CameraBrand.Create(Brand.Dahua, "SD2A500NB");
            cameras.Add(Camera.Create(i.ToString(), cameraBrand, protocol: CameraProtocol.Https));
        }

        foreach (var cam in cameras)
        {
            cam.AddToSite(site);
        }

        foreach (var cam in cameras)
        {
            cam.Created = now;
            cam.CreatedBy = "System";
            cam.LastModified = now;
            cam.LastModifiedBy = "System";
        }

        await dbContext.Cameras.AddRangeAsync(cameras);
        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Seeded {CameraCount} cameras to site 'Regional Office North'.",
            cameras.Count
        );
    }

    private async Task AddIssues()
    {
        if (await dbContext.Issues.AnyAsync())
        {
            logger.LogInformation("Issue seeding skipped: issues already exist.");
            return;
        }

        var sites = await dbContext.Sites
            .Where(site => InvoiceSeedSiteNames.Contains(site.Name.Value))
            .ToDictionaryAsync(site => site.Name.Value);
        var missingSiteNames = InvoiceSeedSiteNames
            .Where(name => !sites.ContainsKey(name))
            .ToArray();
        if (missingSiteNames.Length > 0)
        {
            logger.LogWarning(
                "Issue seeding skipped: seeded sites are missing: {SiteNames}.",
                missingSiteNames);
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;
        var issueDefinitions = new[]
        {
            (
                SiteName: "Central Office",
                Title: "Replace lobby access reader",
                Description: "The lobby access reader intermittently fails to recognize registered cards.",
                Status: IssueStatus.Open,
                StartDate: (DateOnly?)null,
                EndDate: (DateOnly?)null),
            (
                SiteName: "Regional Office North",
                Title: "Review perimeter camera coverage",
                Description: "Confirm that the new loading-area layout remains within camera coverage.",
                Status: IssueStatus.InReview,
                StartDate: today.AddDays(-2),
                EndDate: today.AddDays(3)),
            (
                SiteName: "Regional Office South",
                Title: "Complete emergency lighting inspection",
                Description: "Resolve the findings from the scheduled emergency lighting inspection.",
                Status: IssueStatus.WorkingOn,
                StartDate: today.AddDays(-7),
                EndDate: today.AddDays(7)),
            (
                SiteName: "Central Office",
                Title: "Approve loading dock repair payment",
                Description: "The loading dock repair is complete and awaits payment approval.",
                Status: IssueStatus.ApprovedWaitingPayment,
                StartDate: today.AddDays(-14),
                EndDate: today.AddDays(-1)),
            (
                SiteName: "Regional Office North",
                Title: "Replace server room temperature sensor",
                Description: "The faulty temperature sensor was replaced and the readings are stable.",
                Status: IssueStatus.Completed,
                StartDate: today.AddDays(-10),
                EndDate: today.AddDays(-3)),
        };

        var issues = issueDefinitions.Select(definition =>
        {
            var issue = Issue.Create(
                sites[definition.SiteName],
                definition.Title,
                definition.Description,
                definition.Status);
            issue.UpdateDetails(
                definition.Title,
                definition.Description,
                definition.Status,
                definition.StartDate,
                definition.EndDate);
            issue.Created = now;
            issue.CreatedBy = SeededBy;
            issue.LastModified = now;
            issue.LastModifiedBy = SeededBy;
            return issue;
        }).ToList();

        await dbContext.Issues.AddRangeAsync(issues);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {IssueCount} issues.", issues.Count);
    }

    private async Task AddInvoices(List<Person> persons, int invoiceCount)
    {
        if (persons.Count < 3)
        {
            logger.LogWarning("Invoice seeding skipped: not enough persons available.");
            return;
        }

        var invoiceNumbers = InvoiceSeedData.GetInvoiceNumbers(invoiceCount);
        var existingInvoiceNumbers = await dbContext.Invoices
            .Where(invoice => invoice.CreatedBy == SeededBy
                && invoiceNumbers.Contains(invoice.InvoiceNumber))
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync();
        var invoices = InvoiceSeedData.Create(persons, invoiceCount)
            .Where(invoice => !existingInvoiceNumbers.Contains(invoice.InvoiceNumber))
            .ToList();

        if (invoices.Count == 0)
        {
            logger.LogInformation("Invoice seeding skipped: all seeded invoices already exist.");
            return;
        }

        await dbContext.Invoices.AddRangeAsync(invoices);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {InvoiceCount} invoices.", invoices.Count);
    }

    private async Task AddInvoiceSitePayments(int invoiceCount)
    {
        var sites = await dbContext.Sites
            .Where(site => InvoiceSeedSiteNames.Contains(site.Name.Value))
            .ToListAsync();
        var sitesByName = sites.ToDictionary(site => site.Name.Value);
        var missingSiteNames = InvoiceSeedSiteNames
            .Where(name => !sitesByName.ContainsKey(name))
            .ToArray();
        if (missingSiteNames.Length > 0)
        {
            logger.LogWarning(
                "Invoice allocation seeding skipped: seeded sites are missing: {SiteNames}.",
                missingSiteNames);
            return;
        }

        var orderedSites = InvoiceSeedSiteNames.Select(name => sitesByName[name]).ToArray();
        var invoiceNumbers = InvoiceSeedData.GetInvoiceNumbers(invoiceCount);
        var invoices = await dbContext.Invoices
            .Include(invoice => invoice.SitePayments)
            .Where(invoice => invoice.CreatedBy == SeededBy
                && invoiceNumbers.Contains(invoice.InvoiceNumber))
            .ToListAsync();

        if (invoices.Count == 0)
        {
            logger.LogWarning("Invoice allocation seeding skipped: no seeded invoices are available.");
            return;
        }

        var addedAllocationCount = 0;
        foreach (var invoice in invoices.Where(invoice => invoice.SitePayments.Count == 0))
        {
            if (!invoice.TotalValueIncludingVat.HasValue)
            {
                continue;
            }

            var amounts = SplitAmount(invoice.TotalValueIncludingVat.Value, orderedSites.Length);
            var sitePayments = orderedSites
                .Select((site, index) => SitePayment.Create(
                    invoice,
                    site,
                    amounts[index],
                    SitePaymentDirection.Out))
                .ToList();

            foreach (var sitePayment in sitePayments)
            {
                var now = DateTimeOffset.UtcNow;
                sitePayment.Created = now;
                sitePayment.CreatedBy = SeededBy;
                sitePayment.LastModified = now;
                sitePayment.LastModifiedBy = SeededBy;
            }

            invoice.ReplaceSitePayments(sitePayments);
            dbContext.SitePayments.AddRange(sitePayments);
            addedAllocationCount += sitePayments.Count;
        }

        if (addedAllocationCount == 0)
        {
            logger.LogInformation(
                "Invoice allocation seeding skipped: all seeded invoices already have allocations.");
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded {AllocationCount} invoice site allocations.", addedAllocationCount);
    }

    private static decimal[] SplitAmount(decimal total, int partCount)
    {
        var baseAmount = decimal.Floor(total * 100m / partCount) / 100m;
        var amounts = Enumerable.Repeat(baseAmount, partCount).ToArray();
        amounts[0] += total - amounts.Sum();
        return amounts;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            logger.LogInformation("Starting database initialization...");

            // if (!await dbContext.Database.CanConnectAsync())
            // {
            //     await dbContext.Database.EnsureCreatedAsync();
            //     logger.LogInformation("Database created via EnsureCreated.");

            //     // Verify connectivity after creation
            //     if (!await dbContext.Database.CanConnectAsync())
            //     {
            //         logger.LogError(
            //             "Database could not be created or connected after EnsureCreated."
            //         );
            //         return;
            //     }
            // }
            // else
            // {
            //     // await dbContext.Database.MigrateAsync();
            //     // logger.LogInformation("Applied pending migrations.");
            // }


            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Applied pending migrations.");

            var users = await applicationUserSeedData.SeedAsync();
            await AddSites(users);
            await EnsureThirdSeedSiteAccessAsync(users);
            await AddCameras();
            await AddIssues();
            var persons = await personSeedData.SeedAsync();
            await retailerSeedData.SeedAsync(persons);
            await renovationCatalogSeedData.SeedAsync();
            await activityCatalogSeedData.SeedAsync();
            await retailerExtractionProfileSeedData.SeedAsync(persons);
            var invoiceCount = blobInitializer.GetRequiredSeedInvoiceCount();
            await AddInvoices(persons, invoiceCount);
            await AddInvoiceSitePayments(invoiceCount);
            await blobInitializer.InitializeAsync();

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization.");
            throw;
        }
    }
}
