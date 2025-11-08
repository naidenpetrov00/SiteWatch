using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager
)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

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
        if (users is null || users.Count == 0 || await _dbContext.Sites.AnyAsync())
            return;

        var sites = new List<Site>
        {
            new("Cen", "Vitosha 17")
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

    public async Task InitalizeDatabaseAsync()
    {
        try
        {
            if (!await _dbContext.Database.CanConnectAsync())
            {
                await _dbContext.Database.EnsureCreatedAsync();
                Console.WriteLine("Database created (EnsureCreated).");
            }
            else
            {
                await _dbContext.Database.MigrateAsync();
                Console.WriteLine("Applied pending migrations.");
            }

            var users = await AddUsers();
            await AddSites(users);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
