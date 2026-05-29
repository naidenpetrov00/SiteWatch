using Application.Identity.Commands;
using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Domain.Entities;

namespace Infrastructure.Identity.Services;

public class IdentityService(
    IIdentityAuthenticationService authenticationService,
    IIdentityUserService userService,
    IIdentityVerificationService verificationService
) : IIdentityService
{
    public Task<string?> GetUserNameAsync(string userId) => userService.GetUserNameAsync(userId);

    public Task<IdentityResultModel> CreateUserAsync(
        string userName,
        string email,
        string password
    ) => userService.CreateUserAsync(userName, email, password);

    public Task<bool> IsInRoleAsync(string userId, string role) =>
        userService.IsInRoleAsync(userId, role);

    public Task<bool> AuthorizeAsync(string userId, string policyName) =>
        authenticationService.AuthorizeAsync(userId, policyName);

    public Task<IdentityResultModel> AssignAdministratorClaimAsync(string userId) =>
        userService.AssignAdministratorClaimAsync(userId);

    public Task<Result> DeleteUserAsync(string userId) => userService.DeleteUserAsync(userId);

    public Task<Result> DeleteUserAsync(ApplicationUser user) => userService.DeleteUserAsync(user);

    public Task<ApplicationUser?> FindUserByEmailAsync(string email) =>
        userService.FindUserByEmailAsync(email);

    public Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password) =>
        authenticationService.CheckPasswordAsync(user, password);

    public Task<IdentityResultModel> CheckAdministratorPasswordAsync(
        ApplicationUser user,
        string password
    ) => authenticationService.CheckAdministratorPasswordAsync(user, password);

    public Task<IdentityResultModel> ConfirmEmailAsync(ApplicationUser user, string token) =>
        verificationService.ConfirmEmailAsync(user, token);

    public string GenerateVerificationToken() => verificationService.GenerateVerificationToken();

    public Task<bool> IsVerifiedEmailAsync(string email) =>
        verificationService.IsVerifiedEmailAsync(email);

    public Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    ) => verificationService.ResetPasswordAsync(user, token, newPassword);
}
