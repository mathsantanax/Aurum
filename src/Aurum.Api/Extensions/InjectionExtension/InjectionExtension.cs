using Aurum.Api.Services.Auth;
using Aurum.Application.Financial;
using Aurum.Application.Interfaces.Auth;
using Aurum.Infrastructure.Repositories;

namespace Aurum.Api.Extensions.InjectionExtension
{
    public static class InjectionExtension
    {
        public static WebApplicationBuilder AddInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<IFinancialRepository, FinancialRepository>();
            builder.Services.AddScoped<IFinancialUseCases, FinancialUseCases>();
            return builder;
        }
    }
}
