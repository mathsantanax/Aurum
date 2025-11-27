using Aurum.Applications.Services;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Repositories;

namespace Aurum.API.Extensions
{
    public static class AddScoopeds
    {
        // Extension method to add scooped services to the WebApplicationBuilder
        public static WebApplicationBuilder AddScooped(this WebApplicationBuilder builder)
        {
            // Add scooped-specific services here
            // e.g., builder.Services.AddScoped<IMyScoopedService, MyScoopedService>();

            builder.Services.AddScoped<WalletService>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();

            return builder;
        }
    }
}
