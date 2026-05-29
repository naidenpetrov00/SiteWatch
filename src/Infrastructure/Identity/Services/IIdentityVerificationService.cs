using Application.Identity.Commands;
using Application.SeedWork.Models;
using Domain.Entities;

namespace Infrastructure.Identity.Services;

public interface IIdentityVerificationService
{
    string GenerateVerificationToken();

    Task<IdentityResultModel> ConfirmEmailAsync(ApplicationUser user, string emailToken);

    Task<IdentityResultModel> ResetPasswordAsync(
        ApplicationUser user,
        string token,
        string newPassword
    );

    Task<bool> IsVerifiedEmailAsync(string email);
}
