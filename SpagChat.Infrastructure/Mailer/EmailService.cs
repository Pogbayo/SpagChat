﻿using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Infrastructure.Configurations;

namespace SpagChat.Infrastructure.Mailer
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly SMTPSettings _smtpSettings;
        public EmailService(IOptions<SMTPSettings> settings, ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpSettings = settings.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = "This is a plain text version of the email."
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            _logger.LogInformation($"SMTP Host: {_smtpSettings.Host}");
            _logger.LogInformation($"SMTP Port: {_smtpSettings.Port}");
            _logger.LogInformation($"SMTP Username: {_smtpSettings.Username}");
            _logger.LogInformation($"Sending email to {toEmail}");
        }
    }
}
