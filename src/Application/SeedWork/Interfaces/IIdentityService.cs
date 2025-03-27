namespace Application.SeedWork.Interfaces;

using Application.Identity;
using Application.SeedWork.Models;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<IdentityResultModel> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);
}
