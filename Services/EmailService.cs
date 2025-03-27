using System.Net.Mail;
using System.Net;
using CipherJourney.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace CipherJourney.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string verificationToken);
        Task SendFeedbackEmail(string email, string feedback);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendFeedbackEmail(string email, string feedback) {
            string from = _configuration["Email:CipherJourney"];
            string to = _configuration["Email:CipherJourney"];
            string subject = "Feedback message from: " + email + "!";
            string message = feedback;

            using (var emailSender = new MailMessage(from, to))
            {
                emailSender.Subject = subject;
                emailSender.Body = message;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        _configuration["Email:CipherJourney"],
                        _configuration["Email:AppPass"]);

                    try
                    {
                        await client.SendMailAsync(emailSender);
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

        public async Task SendEmailAsync(string email, string verificationToken)
        {
            string from = _configuration["Email:CipherJourney"];
            string to = email;
            string subject = "Verification code for your CipherJourney!";
            string message = "You verification code is: " + verificationToken;

            using (var emailSender = new MailMessage(from, to))
            {
                emailSender.Subject = subject;
                emailSender.Body = message;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(
                        _configuration["Email:CipherJourney"],
                        _configuration["Email:AppPass"]);

                    try
                    {
                        await client.SendMailAsync(emailSender);
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
