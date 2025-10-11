using System.Runtime.CompilerServices;

namespace Finance.Extensions
{
    public static class BuilderExtension
    {
        public static WebApplicationBuilder AddArchitecture (this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();

            return builder;
        }
    }
}
