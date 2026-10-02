using Aurum.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Encodings.Web;

namespace Aurum.Infrastructure.Email
{
    public sealed class IdentityEmailSender(
        Aurum.Application.Interfaces.EmailInterfaces.IEmailSender emailSender,
        ILogger<IdentityEmailSender> logger,
        IConfiguration configuration)
        : IEmailSender<AurumUser>
    {
        public async Task SendConfirmationLinkAsync(AurumUser user, string email, string confirmationLink)
        {
            var callback = new Uri(WebUtility.HtmlDecode(confirmationLink), UriKind.Absolute);
            var callbackParameters = QueryHelpers.ParseQuery(callback.Query);
            if (!callbackParameters.TryGetValue("userId", out var userId) ||
                !callbackParameters.TryGetValue("code", out var code))
            {
                throw new InvalidOperationException("O link de confirmação não contém os parâmetros esperados.");
            }

            var confirmationParameters = new Dictionary<string, string?>
            {
                ["userId"] = userId.ToString(),
                ["code"] = code.ToString(),
                ["email"] = email
            };
            if (callbackParameters.TryGetValue("changedEmail", out var changedEmail))
            {
                confirmationParameters["changedEmail"] = changedEmail.ToString();
            }

            var frontendLink = BuildFrontendLink("confirm-email", confirmationParameters);

            await SendTemplateAsync(email, "Confirme seu e-mail | Aurum", "Welcome-EmailConfirmation.html", new Dictionary<string, string>
            {
                ["{{ConfirmationLink}}"] = HtmlEncoder.Default.Encode(frontendLink),
                ["{{LogoUrl}}"] = "Aurum"
            });
            logger.LogInformation(
                "Enviando e-mail de confirmação para {Email}",
                email);
        }

        public async Task SendPasswordResetCodeAsync(AurumUser user, string email, string resetCode)
        {
            var frontendLink = BuildFrontendLink("reset-password", new Dictionary<string, string?>
            {
                ["email"] = email,
                ["resetCode"] = WebUtility.HtmlDecode(resetCode)
            });

            await SendTemplateAsync(email, "Redefinição de senha | Aurum", "ResetPasswordLink.html", new Dictionary<string, string>
            {
                ["{{ResetPasswordLink}}"] = HtmlEncoder.Default.Encode(frontendLink),
                ["{{LogoUrl}}"] = "Aurum"
            });
            logger.LogInformation(
                "Enviando e-mail de redefinição de senha para {Email}",
                email);
        }

        public async Task SendPasswordResetLinkAsync(AurumUser user, string email, string resetLink)
        {
            var callback = new Uri(WebUtility.HtmlDecode(resetLink), UriKind.Absolute);
            var callbackParameters = QueryHelpers.ParseQuery(callback.Query);
            if (!callbackParameters.TryGetValue("code", out var code))
            {
                throw new InvalidOperationException("O link de redefinição não contém o código esperado.");
            }

            var frontendLink = BuildFrontendLink("reset-password", new Dictionary<string, string?>
            {
                ["email"] = email,
                ["resetCode"] = code.ToString()
            });

            await SendTemplateAsync(email, "Redefinição de senha | Aurum", "ResetPasswordLink.html", new Dictionary<string, string>
            {
                ["{{ResetPasswordLink}}"] = HtmlEncoder.Default.Encode(frontendLink),
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

        private string BuildFrontendLink(string route, Dictionary<string, string?> query)
        {
            var frontendBaseUrl = configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException("Frontend:BaseUrl precisa estar configurado para montar os links dos e-mails.");
            var frontendUri = new Uri(new Uri($"{frontendBaseUrl.TrimEnd('/')}/", UriKind.Absolute), route);
            return QueryHelpers.AddQueryString(frontendUri.ToString(), query);
        }
    }
}
