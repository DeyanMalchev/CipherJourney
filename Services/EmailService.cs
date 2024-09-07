using System.Net.Mail;
using System.Net;

namespace Experiments.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            string from = _configuration["Email:CipherJourney"];
            string to = email;

            using (var messageSenderToReceiver = new MailMessage(from, to))
            {
                messageSenderToReceiver.Subject = subject;
                messageSenderToReceiver.Body = message;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        _configuration["Email:CipherJourney"],
                        _configuration["Email:AppPass"]);

                    try
                    {
                        await client.SendMailAsync(messageSenderToReceiver);
                        Console.WriteLine("Email sent successfully!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email: {ex.Message}");
                        // Consider logging the full exception details or rethrowing
                        throw;
                    }
                }
            }
        }
    }
}
