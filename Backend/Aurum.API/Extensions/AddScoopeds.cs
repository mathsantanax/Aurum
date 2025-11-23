namespace Aurum.API.Extensions
{
    public static class AddScoopeds
    {
        // Extension method to add scooped services to the WebApplicationBuilder
        public static WebApplicationBuilder AddScooped(this WebApplicationBuilder builder)
        {
            // Add scooped-specific services here
            // e.g., builder.Services.AddScoped<IMyScoopedService, MyScoopedService>();

            return builder;
        }
    }
}
