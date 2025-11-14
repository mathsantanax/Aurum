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

            // string de conexão com o banco de dados supabase
            string stringConnection = Environment.GetEnvironmentVariable("DIRECT_URL")!;

            IdentityModelEventSource.ShowPII = true;
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
            // Verifica se a string está nula
            if (string.IsNullOrEmpty(stringConnection))
                throw new Exception("Sem conexão com o banco de dados!");

            // Configura o Dbcontext no DI
            builder.Services.AddDbContext<AurumDbContext>(options => 
                        options.UseNpgsql(stringConnection));
            // Configura o Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AurumDbContext>()
                .AddDefaultTokenProviders();

            // Verificar se a string de key está nula
            if (string.IsNullOrEmpty(Globals.JWT_TOKEN))
                throw new Exception("A variável de ambiente KEY não está definida. Verifique o arquivo .env.");

            var key = Encoding.UTF8.GetBytes(Globals.JWT_TOKEN.Trim());

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
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Globals.JWT_ISSUER,
                        ValidAudience = Globals.JWT_AUDIENCE,
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"❌ Token inválido: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine($"✅ Token validado para {context.Principal?.Identity?.Name}");
                            return Task.CompletedTask;
                        }
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

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            return builder;
        }
    }
}
