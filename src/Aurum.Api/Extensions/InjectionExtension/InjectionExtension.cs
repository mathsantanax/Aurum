using Aurum.Api.Services.Auth;
using Aurum.Application.Interfaces.Auth;

namespace Aurum.Api.Extensions.InjectionExtension
{
    public static class InjectionExtension
    {
        public static WebApplicationBuilder AddInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            return builder;
        }
    }
}
