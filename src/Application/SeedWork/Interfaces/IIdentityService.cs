namespace Application.SeedWork.Interfaces;

using Application.Identity.Commands;
using Application.SeedWork.Models;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<ApplicationUser?> FindUserByEmailAsync(string email);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<IdentityResultModel> CreateUserAsync(string userName, string email, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password);

    Task<IdentityResultModel> ConfirmEmailAsync(ApplicationUser user, string token);
    string GenerateVerificationToken();
    Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    );
}
