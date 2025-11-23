using Aurum.API.Configurations;
using Aurum.Infrastructure.Persistence;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;

namespace Aurum.API.Extensions
{
    public static class AddArchitectures
    {
        // Extensão para adicionar serviços relacionados à arquitetura da aplicação
        public static WebApplicationBuilder AddArchitecture(this WebApplicationBuilder builder)
        {
            // Add architecture-specific services here
            // e.g., Authentication, Database, Repositories, etc.

            // Carregando a configuração do arquivo .env
            DotEnv.Load();

            // verifica se a variável de ambiente está carregada corretamente
            if(string.IsNullOrEmpty(Globals.ConnectionString))
            {
                throw new Exception("A variável de ambiente DEFAULT_CONNECTION não está definida.");
            }

            // Configurando o DbContext com PostgreSQL do Supabase
            builder.Services.AddDbContext<AppDbContext>(opt =>
                        opt.UseNpgsql(Globals.ConnectionString));

            // verifica se as variáveis de de conexão do jwt para autenticação do Supabase estão carregadas corretamente
            if (string.IsNullOrEmpty(Globals.URL_PROJECT) || string.IsNullOrEmpty(Globals.ANON_KEY))
            {
                throw new Exception("As variáveis de ambiente URL_PROJECT ou ANON_KEY não estão definidas.");
            }


            // Configurando a autenticação JWT com Supabase
            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema de autenticação padrão como JWT Bearer
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema de desafio padrão como JWT Bearer
            })

            // Configura o esquema de autenticação JWT Bearer
            .AddJwtBearer(options =>
            {
                // Configura os parâmetros de validação do token JWT
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Valida a chave de assinatura do emissor
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Globals.ANON_KEY)), // Chave de assinatura do emissor
                    ValidateIssuer = true, // Valida o emissor do token
                    ValidIssuer = Globals.URL_PROJECT, // Emissor válido (URL do projeto Supabase)
                    ValidateAudience = true, // Valida o público do token
                    ValidAudience = "authenticated", // Público válido
                    ValidateLifetime = true, // Valida o tempo de vida do token
                    ClockSkew = TimeSpan.Zero // Sem tolerância de tempo
                };
            });

            
            // Adiciona o serviço de exploração de endpoints para APIs
            builder.Services.AddEndpointsApiExplorer();

            // Adiciona o serviço de autorização
            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            return builder;
        }
    }
}
