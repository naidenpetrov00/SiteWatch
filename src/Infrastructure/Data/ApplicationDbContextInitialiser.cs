namespace Infrastructure.Data;

using System;
using System.Threading.Tasks;
using Application.SeedWork.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    private async Task<IList<ApplicationUser>> AddUsers()
    {
        var user1 = new ApplicationUser
        {
            UserName = "Test.2010",
            Email = "naiden.petrov.31.12.00@gmail.com",
            PasswordHash = "Test.2010",
            EmailConfirmed = true,
        };
        var user2 = new ApplicationUser
        {
            UserName = "Test2.2010",
            Email = "naidenpetrov00@gmail.com",
            PasswordHash = "Test2.2010",
            EmailConfirmed = true,
        };
        await _userManager.CreateAsync(user1, "Test@123");
        await _userManager.CreateAsync(user2, "Test@123");
        return [user1, user2];
    }

    private async Task AddSites(IList<ApplicationUser> users)
    {
        if (await _dbContext.Sites.AnyAsync() && users is null)
            return;

        var sites = new List<Site>
        {
            new Site("Central Office")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
                Users = new List<ApplicationUser> { users[0] },
            },
            new Site("Regional Office North")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
                Users = new List<ApplicationUser> { users[1] },
            },
            new Site("Regional Office South")
            {
                Created = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                LastModified = DateTimeOffset.UtcNow,
                LastModifiedBy = "System",
                Users = users,
            },
        };

        await _dbContext.Sites.AddRangeAsync(sites);
        await _dbContext.SaveChangesAsync();
    }

    public ApplicationDbContextInitialiser(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task InitalizeDatabaseAsync()
    {
        try
        {
            await _dbContext.Database.EnsureCreatedAsync();
            await _dbContext.Database.MigrateAsync();
            var users = await AddUsers();
            await AddSites(users);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
