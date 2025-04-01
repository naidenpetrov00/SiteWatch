using System.Net.Mail;
using Application.SeedWork.Models.Internal;

namespace Infrastructure.Email;

public static class EmailTemplates
{
    public static MailMessage VerifyEmail(string from, string to,string token) =>
        new()
        {
            From = new MailAddress(from),
            Subject = "SiteWatch. Email verification Token",
            Body = $"Your verificatin token: ${token}",
        };
}
