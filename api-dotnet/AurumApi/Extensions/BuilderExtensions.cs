using AurumApi.Persistence;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AurumApi.Extensions
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            // Carregar variaveis do .env
            DotEnv.Load();

            //ler configurações
            string direct_url = Environment.GetEnvironmentVariable("DIRECT_URL")!;

            if (string.IsNullOrEmpty(direct_url))
                builder.Services.AddDbContext<AurumDbContext>(options => 
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<AurumDbContext>(options => 
                options.UseNpgsql(direct_url));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            return builder;
        }
    }
}
