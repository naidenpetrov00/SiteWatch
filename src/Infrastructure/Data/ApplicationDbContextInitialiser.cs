namespace Infrastructure.Data;

using System;
using System.Threading.Tasks;
using Application.SeedWork.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

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
            await _dbContext.Database.MigrateAsync();
            var result = await this.AddUsers();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Task<IdentityResult> AddUsers()
    {
        var user = new ApplicationUser { UserName = "test", Email = "test@email.com" };
        return _userManager.CreateAsync(user, "Test@123");
    }
}
