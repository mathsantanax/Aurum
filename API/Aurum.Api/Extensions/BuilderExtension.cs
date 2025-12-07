using Aurum.Domain.Entities;
using Aurum.Infrastructure.Persistence;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Aurum.Api.Configuration;
using Microsoft.IdentityModel.Logging;


namespace Aurum.Api.Extensions
{
    public static class BuilderExtension
    {
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            // Carrega Variaveis do .env
            DotEnv.Load();

            builder.Services.AddControllers();
            
            // string de conexão com o banco de dados supabase
            string stringConnection = Environment.GetEnvironmentVariable("DIRECT_URL")!;

            // Verifica se a string está nula
            if (string.IsNullOrEmpty(stringConnection))
                throw new Exception("Sem conexão com o banco de dados!");

            // Configura o Dbcontext no DI
            builder.Services.AddDbContext<AurumDbContext>(options => 
                        options.UseNpgsql(stringConnection));

            // Configura o Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;

            })
                .AddEntityFrameworkStores<AurumDbContext>()
                .AddDefaultTokenProviders();

            // Verificar se a string de key está nula
            if (string.IsNullOrEmpty(Globals.JWT_TOKEN))
                throw new Exception("A variável de ambiente KEY não está definida. Verifique o arquivo .env.");


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Globals.JWT_TOKEN)),
                        ValidIssuer = Globals.JWT_ISSUER,
                        ValidAudience = Globals.JWT_AUDIENCE,
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
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
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

            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
