using Aurum.Infrastructure.Persistence;
using dotenv.net;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Extensions
{
    public static class BuilderExtension
    {
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            DotEnv.Load();

            string stringConnection = Environment.GetEnvironmentVariable("DIRECT_URL")!;

            if(string.IsNullOrEmpty(stringConnection))
                throw new Exception("Sem conexão com o banco de dados!");

            builder.Services.AddDbContext<AurumDbContext>(options => 
                        options.UseNpgsql(stringConnection));

            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            return builder;
        }
    }
}
