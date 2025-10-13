using Aurum_Application.Interfaces;
using Aurum_Application.Service;
using Aurum_Domain.Interfaces;
using Aurum_Infrastructure.Repositories;

namespace AurumApi.Extensions
{
    public static class AddScoped
    {
        public static WebApplicationBuilder AddScopedArchitecture(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            return builder;
        }
    }
}
