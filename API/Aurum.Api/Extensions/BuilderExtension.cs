using Aurum.Domain.Entities;
using Aurum.Infrastructure.Persistence;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


namespace Aurum.Api.Extensions
{
    public static class BuilderExtension
    {
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            DotEnv.Load();

            // string de conexão com o banco de dados supabase
            string stringConnection = Environment.GetEnvironmentVariable("DIRECT_URL")!;
            // key jwt
            string? jwtToken = Environment.GetEnvironmentVariable("KEY");

            // Configura o Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                            .AddEntityFrameworkStores<AurumDbContext>()
                            .AddDefaultTokenProviders();

            // Verifica se a string está nula
            if (string.IsNullOrEmpty(stringConnection))
                throw new Exception("Sem conexão com o banco de dados!");

            // Verificar se a string de key está nula
            if(string.IsNullOrEmpty(jwtToken))
                throw new Exception("A variável de ambiente JWT_TOKEN não está definida. Verifique o arquivo .env.");

            // Configura o Dbcontext no DI
            builder.Services.AddDbContext<AurumDbContext>(options => 
                        options.UseNpgsql(stringConnection));

            var key = Encoding.ASCII.GetBytes(jwtToken);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = Environment.GetEnvironmentVariable("ISSUER"),
                        ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE"),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Aurum Seu Gerenciador Financeiro", Version = "v1" });

                //configuração de autenticação no swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Digite 'Bearer {seu_token}' para autenticar."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            return builder;
        }
    }
}
