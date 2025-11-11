using System.Net;
using System.Net.Mail;
using Application.SeedWork.Enums;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Domain.Entities;
using Infrastructure.Data.Options;
using Infrastructure.SeedWork.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly GmailOptions _gmailOptions;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailService(IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _gmailOptions = Guard.Against.Null(config.GetOptions<GmailOptions>());
        _userManager = userManager;
    }

    private async Task SendEmail(MailMessage message)
    {
        var password = Environment.GetEnvironmentVariable("GmailSmtpPassword");
        // odnk ghjs mqvm swlg

        using var client = new SmtpClient();
        client.Host = _gmailOptions.Host!;
        client.Port = int.Parse(_gmailOptions.Port!);
        client.Credentials = new NetworkCredential(_gmailOptions.Email, password);
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;

        await client.SendMailAsync(message);
    }

    public async Task SendVerifyEmailAsync(ApplicationUser user, string toEmail, string token)
    {
        await _userManager.SetAuthenticationTokenAsync(
            user,
            EmailProvider.Email.ToString(),
            EmailProvider.SMTP.ToString(),
            token
        );
        var message = EmailTemplates.VerifyEmail(_gmailOptions.Email!, toEmail, token);

        await SendEmail(message);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string token)
    {
        var message = EmailTemplates.ResetPassword(_gmailOptions.Email!, toEmail, token);
        await SendEmail(message);
    }
}
