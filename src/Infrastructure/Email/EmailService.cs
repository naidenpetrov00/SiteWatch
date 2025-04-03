using System.Net;
using System.Net.Mail;
using Application.SeedWork.Interfaces;
using Ardalis.GuardClauses;
using Infrastructure.Data.Options;
using Infrastructure.Email;
using Infrastructure.SeedWork.Options;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly GmailOptions _gmailOptions;

    public EmailService(IConfiguration config)
    {
        _gmailOptions = Guard.Against.Null(config.GetOptions<GmailOptions>());
    }

    private async Task SendEmail(MailMessage message)
    {
        var password = Environment.GetEnvironmentVariable("GmailSmtpPassword");

        using var client = new SmtpClient();
        client.Host = _gmailOptions.Host!;
        client.Port = int.Parse(_gmailOptions.Port!);
        client.Credentials = new NetworkCredential(_gmailOptions.Email, password);
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;

        await client.SendMailAsync(message);
    }

    public async Task SendVerifyEmailAsync(string toEmail, string token)
    {
        var message = EmailTemplates.VerifyEmail(_gmailOptions.Email!, toEmail, token);
        await SendEmail(message);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string token)
    {
        var message = EmailTemplates.ResetPassword(_gmailOptions.Email!, toEmail, token);
        await SendEmail(message);
    }
}
