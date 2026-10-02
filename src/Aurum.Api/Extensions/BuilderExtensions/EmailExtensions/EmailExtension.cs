using Aurum.Application.Interfaces.EmailInterfaces;
using Aurum.Infrastructure.Email;
using Aurum.Infrastructure.Email.Configuration;
using Aurum.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurum.Api.Extensions.BuilderExtensions.EmailExtensions
{
    public static class EmailExtension
    {
        public static WebApplicationBuilder AddEmailConfig(
       this WebApplicationBuilder builder)
        {
            var smtpOptions = new SmtpOptions
            {
                Host = Environment.GetEnvironmentVariable("EMAIL_HOST")
                       ?? "smtp.gmail.com",

                Port = int.TryParse(
                    Environment.GetEnvironmentVariable("EMAIL_PORT"),
                    out var port)
                    ? port
                    : 587,

                User = Environment.GetEnvironmentVariable("EMAIL_USER")
                       ?? string.Empty,

                Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                           ?? string.Empty,

                EnableSsl = bool.TryParse(
                    Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL"),
                    out var enableSsl)
                    ? enableSsl
                    : true,

                FromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM_EMAIL")
                            ?? string.Empty,

                FromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME")
                           ?? "Aurum"
            };

            builder.Services.AddSingleton(
                Microsoft.Extensions.Options.Options.Create(smtpOptions));

            builder.Services.AddSingleton<Aurum.Application.Interfaces.EmailInterfaces.IEmailSender, SmtpEmailSender>();

            builder.Services.RemoveAll<
            Microsoft.AspNetCore.Identity.IEmailSender<AurumUser>>();

            builder.Services.AddSingleton<
                Microsoft.AspNetCore.Identity.IEmailSender<AurumUser>,
                IdentityEmailSender>();

            return builder;
        }
    }
}
