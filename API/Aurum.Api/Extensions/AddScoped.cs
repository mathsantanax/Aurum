using Aurum.Application.Services;
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Repositories;

namespace Aurum.Api.Extensions
{
    public static class AddScoped
    {
        public static WebApplicationBuilder UseScoped(this WebApplicationBuilder builder)
        {
            // Usuário
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<UserService>();

            // Carteira Privada
            builder.Services.AddScoped<IPrivateWalletRepository, PrivateWalletRepository>();
            builder.Services.AddScoped<PrivateWalletService>();

            return builder;
        }
    }
}
