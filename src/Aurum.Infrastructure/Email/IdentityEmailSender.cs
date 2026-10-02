using Aurum.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Aurum.Infrastructure.Email
{
    public sealed class IdentityEmailSender(
        Aurum.Application.Interfaces.EmailInterfaces.IEmailSender emailSender,
        ILogger<IdentityEmailSender> logger)
        : IEmailSender<AurumUser>
    {
        public async Task SendConfirmationLinkAsync(AurumUser user, string email, string confirmationLink)
        {
            await SendTemplateAsync(email, "Confirme seu e-mail | Aurum", "Welcome-EmailConfirmation.html", new Dictionary<string, string>
            {
                ["{{ConfirmationLink}}"] = confirmationLink,
                ["{{LogoUrl}}"] = "Aurum"
            });
            logger.LogInformation(
                "Enviando e-mail de confirmação para {Email}",
                email);
        }

        public async Task SendPasswordResetCodeAsync(AurumUser user, string email, string resetCode)
        {
            await SendTemplateAsync(email, "Redefinição de senha | Aurum", "ResetPasswordLink.html", new Dictionary<string, string>
            {
                ["{{ResetPasswordLink}}"] = resetCode,
                ["{{LogoUrl}}"] = "Aurum"
            });
            logger.LogInformation(
                "Enviando e-mail de redefinição de senha para {Email}",
                email);
        }

        public async Task SendPasswordResetLinkAsync(AurumUser user, string email, string resetLink)
        {
            await SendTemplateAsync(email, "Redefinição de senha | Aurum", "ResetPasswordLink.html", new Dictionary<string, string>
            {
                ["{{ResetPasswordLink}}"] = resetLink,
                ["{{LogoUrl}}"] = "Aurum"
            });
            logger.LogInformation(
                "Enviando e-mail de redefinição de senha para {Email}",
                email);
        }

        private async Task SendTemplateAsync(string email, string subject, string templateName, Dictionary<string, string> replacements)
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Email", "Templates", templateName);
            var html = await File.ReadAllTextAsync(templatePath);
            html = replacements.Aggregate(html, (content, replacement) => content.Replace(replacement.Key, replacement.Value));
            html = html.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
            await emailSender.SendAsync(email, subject, html);
        }
    }
}
