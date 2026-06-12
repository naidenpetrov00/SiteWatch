using Application.SeedWork.Security;
using Domain.Entities;
using Domain.SeedWork.Enums;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<ApplicationDbContextInitialiser> logger
)
{
    private static readonly string[] SeedUserEmails =
    [
        "naiden.petrov.31.12.00@gmail.com",
        "naidenpetrov00@gmail.com",
    ];

    private const string BulkSeedEmailDomain = "sitewatch.local";
    private const int BulkSeedUserCount = 5000;

    private async Task<List<ApplicationUser>> AddUsers()
    {
        if (await userManager.Users.AnyAsync())
        {
            logger.LogInformation("User seeding skipped: users already exist.");
            return await userManager.Users.ToListAsync();
        }

        var now = DateTimeOffset.UtcNow;

        var user1 = new ApplicationUser
        {
            UserName = "Test.2010",
            Email = "naiden.petrov.31.12.00@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+359888000001",
            PhoneNumberConfirmed = true,
            LastLoginAt = now.AddDays(-1),
        };
        var user2 = new ApplicationUser
        {
            UserName = "Test2.2010",
            Email = "naidenpetrov00@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+359888000002",
            PhoneNumberConfirmed = true,
            LastLoginAt = now.AddHours(-4),
        };
        await userManager.CreateAsync(user1, "Test@123");
        await userManager.CreateAsync(user2, "Test@123");

        await userManager.AddClaimAsync(
            user1,
            new Claim(UserClaimTypes.UserType, UserClaimTypes.Administrator)
        );
        await userManager.AddClaimAsync(
            user2,
            new Claim(UserClaimTypes.UserType, UserClaimTypes.Administrator)
        );

        var users = new List<ApplicationUser> { user1, user2 };

        for (var i = 1; i <= BulkSeedUserCount; i++)
        {
            var bulkUser = new ApplicationUser
            {
                UserName = $"user{i:0000}",
                Email = $"user{i:0000}@{BulkSeedEmailDomain}",
                EmailConfirmed = i % 2 == 0,
                PhoneNumber = $"+359888{i:000000}",
                PhoneNumberConfirmed = i % 3 == 0,
                LastLoginAt = i % 5 == 0 ? now.AddDays(-(i % 30)) : null,
            };

            var result = await userManager.CreateAsync(bulkUser, "Test@123");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning(
                    "Failed to create seed user {Email}: {Errors}",
                    bulkUser.Email,
                    errors
                );
                continue;
            }

            await userManager.AddClaimAsync(
                bulkUser,
                new Claim(UserClaimTypes.UserType, UserClaimTypes.Client)
            );

            users.Add(bulkUser);
        }

        logger.LogInformation("Seeded {UserCount} users.", users.Count);
        return users;
    }

    private async Task AddSites(List<ApplicationUser> users)
    {
        if (users.Count == 0 || await dbContext.Sites.AnyAsync())
            return;

        var sites = new List<Site>
        {
            new("Central Office", "Vitosha 17")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
            },
            new("Regional Office North", "Dondukov 11")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
            },
            new("Regional Office South", "Kestenova Gora 24")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
            },
        };
        sites[0].AddUser(users[0]);
        sites[1].AddUser(users[1]);
        sites[1].AddUserRange(users);

        await dbContext.Sites.AddRangeAsync(sites);
        await dbContext.SaveChangesAsync();
    }

    private async Task ClearSeedDataAsync()
    {
        var deletedCameras = await dbContext.Cameras.ExecuteDeleteAsync();
        var deletedSites = await dbContext.Sites.ExecuteDeleteAsync();

        if (deletedCameras > 0 || deletedSites > 0)
        {
            logger.LogInformation(
                "Cleared {CameraCount} cameras and {SiteCount} sites.",
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

        var site = await dbContext.Sites.FirstOrDefaultAsync(s => s.Address.Value == "Dondukov 11");

        if (site is null)
        {
            logger.LogWarning("Camera seeding skipped: site with address 'Dondukov 11' not found.");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var cameras = new List<Camera>();
        for (var i = 1; i <= 5; i++)
        {
            var cameraBrand = CameraBrand.Create(Brand.Dahua, "SD2A500NB");
            cameras.Add(Camera.Create(i.ToString(), cameraBrand));
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
            "Seeded {CameraCount} cameras to site at address 'Dondukov 11'.",
            cameras.Count
        );
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

            var users = await AddUsers();
            await AddSites(users);
            await AddCameras();

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization.");
            throw;
        }
    }
}
