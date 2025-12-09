using Aurum.Applications.Services;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Data;
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

            builder.Services.AddScoped<AppDbContext>();

            builder.Services.AddScoped<WalletService>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();

            builder.Services.AddScoped<CreditCardService>();
            builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();

            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            return builder;
        }
    }
}
