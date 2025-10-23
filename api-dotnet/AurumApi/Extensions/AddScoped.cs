

using AurumApi.Interfaces;
using AurumApi.Repositories;
using AurumApi.Service;

namespace AurumApi.Extensions
{
    public static class AddScoped
    {
        public static WebApplicationBuilder AddScopedArchitecture(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserInterface, UserRepository>();
            builder.Services.AddScoped<UserService>();
            return builder;
        }
    }
}
