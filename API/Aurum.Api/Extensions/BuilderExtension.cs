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

            // Verifica se a string está nula
            if (string.IsNullOrEmpty(Globals.CONNECTION_STRING))
                throw new Exception("Sem conexão com o banco de dados!");

            // Configura o Dbcontext no DI
            builder.Services.AddDbContext<AurumDbContext>(options => 
                        options.UseNpgsql(Globals.CONNECTION_STRING));
            // Configura o Identity
            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AurumDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    if (string.IsNullOrEmpty(Globals.JWT_TOKEN))
                        throw new Exception("Chave JWT não definida!");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Globals.JWT_ISSUER,
                        ValidAudience = Globals.JWT_AUDIENCE,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Globals.JWT_TOKEN)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Aurum API", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT (Emitido pelo Supabase) no formato: Bearer SEU_TOKEN_AQUI",
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
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            return builder;
        }
    }
}
