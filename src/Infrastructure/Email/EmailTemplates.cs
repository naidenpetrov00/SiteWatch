using System.Net.Mail;

namespace Infrastructure.Email;

public static class EmailTemplates
{
    public static MailMessage VerifyEmail(string from, string to, string token)
    {
        var subject = "SiteWatch. Email verification Token";
        var body = $"Your verificatin token: {token}";
        return new MailMessage(from, to, subject, body);
    }
}
