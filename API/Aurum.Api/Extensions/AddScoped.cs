using Aurum.Application.Services;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Repositories;

namespace Aurum.Api.Extensions
{
    public static class AddScoped
    {
        public static WebApplicationBuilder UseScoped(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthService>();

            //Registros

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<PrivateWalletService>();
            builder.Services.AddScoped<IPrivateWalletRepositories, PrivateWalletRepository>();

            return builder;
        }
    }
}
