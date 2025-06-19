using MinM_API.Services.Interfaces;
using System.Net.Mail;
using System.Net;

namespace MinM_API.Services.Implementations
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = config.GetSection("EmailSettings");

            var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
            {
                Port = int.Parse(emailSettings["Port"]),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
