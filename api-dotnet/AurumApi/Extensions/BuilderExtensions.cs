namespace AurumApi.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            return builder;
        }
    }
}
