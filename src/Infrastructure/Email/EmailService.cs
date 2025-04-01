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

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var password = Environment.GetEnvironmentVariable("GmailSmtpPassword");
        using var client = new SmtpClient()
        {
            Host = _gmailOptions.Host!,
            Port = int.Parse(_gmailOptions.Port!),
            Credentials = new NetworkCredential(_gmailOptions.Email, password),
            EnableSsl = true,
        };
        var message = EmailTemplates.VerifyEmail(_gmailOptions.Email, toEmail);
        // var message = new MailMessage(_gmailOptions.Email!, toEmail, subject, body);

        await client.SendMailAsync(message);
    }
}
