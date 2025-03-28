namespace Application.SeedWork.Interfaces;

using Application.Identity;
using Application.SeedWork.Models;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<ApplicationUser> FindUserByEmailAsync(string email);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<IdentityResultModel> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<IdentityResultModel> CheckPasswordAsync(IApplicationUser user, string password);
}
