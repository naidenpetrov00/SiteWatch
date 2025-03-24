namespace Infrastructure.Data;

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext dbContext;

    public ApplicationDbContextInitialiser(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task InitalizeDatabaseAsync()
    {
        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
