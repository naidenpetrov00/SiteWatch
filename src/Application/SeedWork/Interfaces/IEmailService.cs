using Domain.Entities;

namespace Application.SeedWork.Interfaces;

public interface IEmailService
{
    Task SendVerifyEmailAsync(ApplicationUser user, string toEmail, string token);
    Task SendPasswordResetEmailAsync(string toEmail, string token);
}
