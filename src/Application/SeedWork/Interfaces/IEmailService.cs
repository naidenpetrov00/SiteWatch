namespace Application.SeedWork.Interfaces;

public interface IEmailService
{
    Task SendVerifyEmailAsync(string toEmail, string token);
    Task SendPasswordResetEmailAsync(string toEmail, string token);
}
