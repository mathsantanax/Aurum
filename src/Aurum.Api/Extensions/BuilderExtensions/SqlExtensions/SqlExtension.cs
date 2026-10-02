using Aurum.Infrastructure.Persistence;
using dotenv.net;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Extensions.BuilderExtensions.SqlExtensions
{
    public static class SqlExtension
    {
        public static WebApplicationBuilder AddSqlConfig(this WebApplicationBuilder builder)
        {
            DotEnv.Load();

            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
            var encrypt = Environment.GetEnvironmentVariable("DB_ENCRYPT") ?? "";
            var trustServerCertificate = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE") ?? "";
            var connectionTimeout = Environment.GetEnvironmentVariable("DB_CONNECTION_TIMEOUT") ?? "";

            var connectionString = $"Server={host},{port};Database={database};User Id={user};Password={password};Encrypt={encrypt};TrustServerCertificate={trustServerCertificate};Connection Timeout={connectionTimeout};";

            // Add the DbContext to the service collection with the connection string
            builder.Services.AddDbContext<AurumDbContext>(options =>
                options.UseSqlServer(connectionString));

            return builder;
        }
    }
}
