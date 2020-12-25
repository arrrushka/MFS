using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfiguration;

        public EmailSender(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public async Task SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendEmailMessage(emailMessage);
        }

      

        public MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var messageBody = new BodyBuilder { HtmlBody = message.Content };

            if (!string.IsNullOrWhiteSpace(message.Attachment))
            {
                var file = new FileInfo(message.Attachment);
                messageBody.Attachments.Add(file.Name, File.ReadAllBytes(message.Attachment));
            }

            emailMessage.Body = messageBody.ToMessageBody();

            return emailMessage;
        }

        private async Task SendEmailMessage(MimeMessage emailMessage)
        {
            using(var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password);
                    await client.SendAsync(emailMessage);
                }
                catch(Exception)
                {
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
