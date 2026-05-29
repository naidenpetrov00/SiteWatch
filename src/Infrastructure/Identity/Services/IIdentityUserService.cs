using Application.Identity.Commands;
using Application.SeedWork.Models;
using Domain.Entities;

namespace Infrastructure.Identity.Services;

public interface IIdentityUserService
{
    Task<IdentityResultModel> AssignAdministratorClaimAsync(string userId);

    Task<IdentityResultModel> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<Result> DeleteUserAsync(ApplicationUser user);

    Task<string?> GetUserNameAsync(string userId);

    Task<ApplicationUser?> FindUserByEmailAsync(string email);

    Task<bool> IsInRoleAsync(string userId, string role);
}
