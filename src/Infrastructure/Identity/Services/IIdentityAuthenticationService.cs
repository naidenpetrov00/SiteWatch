using Application.Identity.Commands;
using Application.SeedWork.Models;
using Domain.Entities;

namespace Infrastructure.Identity.Services;

public interface IIdentityAuthenticationService
{
    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<IdentityResultModel> CheckAdministratorPasswordAsync(
        ApplicationUser user,
        string password
    );

    Task<IdentityResultModel> CheckPasswordAsync(ApplicationUser user, string password);
}
