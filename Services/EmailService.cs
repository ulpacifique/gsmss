using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace CommunityFinanceAPI.Utilities
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"]!;
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
            _senderEmail = _configuration["EmailSettings:SenderEmail"]!;
            _senderPassword = _configuration["EmailSettings:SenderPassword"]!;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName)
        {
            var subject = "Welcome to Community Finance Group!";
            var body = $@"
                <h2>Welcome, {recipientName}!</h2>
                <p>Thank you for joining our Community Finance Group. We're excited to have you on board!</p>
                <p>You can now:</p>
                <ul>
                    <li>View and join savings goals</li>
                    <li>Make contributions towards your goals</li>
                    <li>Track your progress</li>
                </ul>
                <p>If you have any questions, please don't hesitate to contact us.</p>
                <br>
                <p>Best regards,<br>The Community Finance Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendContributionNotificationAsync(string recipientEmail, string recipientName, string goalName, decimal amount, string status)
        {
            var subject = $"Contribution {status} - {goalName}";
            var body = $@"
                <h2>Contribution Update</h2>
                <p>Hello {recipientName},</p>
                <p>Your contribution of <strong>{amount:C}</strong> towards <strong>{goalName}</strong> has been <strong>{status}</strong>.</p>
                <p>Thank you for your contribution!</p>
                <br>
                <p>Best regards,<br>The Community Finance Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string recipientEmail, string temporaryPassword)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset</h2>
                <p>You have requested to reset your password.</p>
                <p>Your temporary password is: <strong>{temporaryPassword}</strong></p>
                <p>Please log in with this temporary password and change it immediately.</p>
                <p><strong>Note:</strong> For security reasons, please change your password after logging in.</p>
                <br>
                <p>Best regards,<br>The Community Finance Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}