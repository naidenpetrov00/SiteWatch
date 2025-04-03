namespace Application.SeedWork.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string token);
}
