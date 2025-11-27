using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<ApplicationDbContextInitialiser> logger
)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;

    private async Task<List<ApplicationUser>> AddUsers()
    {
        var user1 = new ApplicationUser
        {
            UserName = "Test.2010",
            Email = "naiden.petrov.31.12.00@gmail.com",
            EmailConfirmed = true,
        };
        var user2 = new ApplicationUser
        {
            UserName = "Test2.2010",
            Email = "naidenpetrov00@gmail.com",
            EmailConfirmed = true,
        };
        await _userManager.CreateAsync(user1, "Test@123");
        await _userManager.CreateAsync(user2, "Test@123");
        return [user1, user2];
    }

    private async Task AddSites(List<ApplicationUser> users)
    {
        if (users.Count == 0 || await _dbContext.Sites.AnyAsync())
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

        await _dbContext.Sites.AddRangeAsync(sites);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AddCameras()
    {
        if (await _dbContext.Cameras.AnyAsync())
        {
            _logger.LogInformation("Camera seeding skipped: cameras already exist.");
            return;
        }

        var site = await _dbContext.Sites
            .FirstOrDefaultAsync(s => s.Address.Value == "Dondukov 11");

        if (site is null)
        {
            _logger.LogWarning("Camera seeding skipped: site with address 'Dondukov 11' not found.");
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var cameras = new List<Camera>
        {
            Camera.Create("Entrance"),
            Camera.Create("Lobby"),
            Camera.Create("Parking"),
        };

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

        await _dbContext.Cameras.AddRangeAsync(cameras);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Seeded {CameraCount} cameras to site at address 'Dondukov 11'.", cameras.Count);
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            if (!await _dbContext.Database.CanConnectAsync())
            {
                await _dbContext.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database created via EnsureCreated.");

                // Verify connectivity after creation
                if (!await _dbContext.Database.CanConnectAsync())
                {
                    _logger.LogError("Database could not be created or connected after EnsureCreated.");
                    return;
                }
            }
            else
            {
                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("Applied pending migrations.");
            }

            var users = await AddUsers();
            await AddSites(users);
            await AddCameras();

            _logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization.");
            throw;
        }
    }
}