using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Rocky_Utilitu
{
    public class EmailSender : IEmailSender
    {
        private  readonly IConfiguration _config;
        public MailJetSettings _mailJetSettings { get; set; }

        public EmailSender(IConfiguration configuration, IConfiguration config)
        {
            _config = config;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            _mailJetSettings = _config.GetSection("MailSettings").Get<MailJetSettings>();

            var send = new MimeMessage();
            send.From.Add(MailboxAddress.Parse(_mailJetSettings.From));
            send.To.Add(MailboxAddress.Parse(email));
            send.Subject = subject;

            send.Body = new TextPart(TextFormat.Html) { Text = body };

            // send email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailJetSettings.Host, _mailJetSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailJetSettings.UserName, _mailJetSettings.Password);
            await smtp.SendAsync(send);
            await smtp.DisconnectAsync(true);

        }
    }
}
