using Aurum.Application.Interfaces.EmailInterfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Aurum.Infrastructure.Email.Configuration
{
    public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
    {
            private readonly SmtpOptions _options = options.Value;
        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _options.FromName,
                    _options.FromEmail));

            message.To.Add(
            MailboxAddress.Parse(to));

            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                var secureSocketOptions = _options.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(
                    _options.Host,
                    _options.Port,
                    secureSocketOptions,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(_options.User) &&
                    !string.IsNullOrWhiteSpace(_options.Password))
                {
                    await client.AuthenticateAsync(
                        _options.User,
                        _options.Password,
                        cancellationToken);
                }
                else if (!string.IsNullOrWhiteSpace(_options.User) ||
                         !string.IsNullOrWhiteSpace(_options.Password))
                {
                    throw new InvalidOperationException(
                        "EMAIL_USER e EMAIL_PASSWORD precisam ser configurados juntos.");
                }

                await client.SendAsync(
                    message,
                    cancellationToken);

                await client.DisconnectAsync(
                    true,
                    cancellationToken);

                logger.LogInformation(
                    "E-mail enviado para {Email} com assunto {Subject}",
                    to,
                    subject);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro ao enviar e-mail para {Email}",
                    to);

                throw;
            }
        }
    }
}
