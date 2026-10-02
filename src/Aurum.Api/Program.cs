using Aurum.Api.Extensions.AppExtensions.RegistrationExtensions;
using Aurum.Api.Extensions.BuilderExtensions.AuthExtension;
using Aurum.Api.Extensions.BuilderExtensions.EmailExtensions;
using Aurum.Api.Extensions.BuilderExtensions.SecurityExtensions;
using Aurum.Api.Extensions.BuilderExtensions.SqlExtensions;
using Aurum.Api.Extensions.InjectionExtension;
using Aurum.Infrastructure.Identity;
using Aurum.Infrastructure.Persistence;
using Aurum.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            builder.AddSecurity(); // Add security configurations
            builder.AddSqlConfig(); // Add SQL configuration
            builder.AddIdentityConfig(); // Add Identity configuration
            builder.AddEmailConfig(); // Add email configuration
            builder.AddInjection(); // Add dependency injection configuration

            builder.Services.AddControllers(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
            });

            var app = builder.Build();

            using(var scope = app.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<AurumDbContext>();
                await database.Database.MigrateAsync(); // Apply any pending migrations to the database
                await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider); // Seed roles into the database
            }

            var allowedOrigins =
                builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>(); // Get allowed origins from configuration

            // Configure exception handling middleware
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var origin = context.Request.Headers.Origin.ToString();
                    if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
                    {
                        context.Response.Headers.AccessControlAllowOrigin = origin;
                        context.Response.Headers.AccessControlAllowCredentials = "true";
                    }

                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    app.Logger.LogError(exceptionFeature?.Error, "Erro não tratado ao processar {Path}", context.Request.Path);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Title = "Ocorreu um erro inesperado ao processar a requisição.",
                        Status = StatusCodes.Status500InternalServerError
                    }); // Return a generic error response
                });
            });// Configure CORS middleware

            // Configure CORS middleware
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            }); // Use forwarded headers middleware

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aurum API v1");
                    options.RoutePrefix = string.Empty; // Set Swagger UI at the root
                });
            }

            app.UseHttpsRedirection(); // Use HTTPS redirection middleware

            app.UseCors("AurumFrontend"); // Use CORS policy

            app.UseRateLimiter(); // Use rate limiting middleware

            app.UseRegistrationGuard(); // Use registration guard middleware

            // Authentication/authorization must run before the CSRF check below:
            // antiforgery tokens are bound to the authenticated ClaimsPrincipal, so
            // HttpContext.User needs to be populated (by UseAuthentication) before
            // ValidateRequestAsync runs. Otherwise every authenticated mutation fails
            // with "the antiforgery token was meant for a different claims-based user".
            app.UseAuthentication(); // Use authentication middleware
            app.UseAuthorization(); // Use authorization middleware

            app.Use(async (context, next) =>
            {
                var method = context.Request.Method;
                var isSafeMethod =
                    HttpMethods.IsGet(method) ||
                    HttpMethods.IsHead(method) ||
                    HttpMethods.IsOptions(method) ||
                    HttpMethods.IsTrace(method);
                var isPublicIdentityRequest =
                    context.Request.Path.StartsWithSegments("/api/auth/login") ||
                    context.Request.Path.StartsWithSegments("/api/auth/register");
                var isApiRequest = context.Request.Path.StartsWithSegments("/api");

                if (!isApiRequest || isSafeMethod || isPublicIdentityRequest)
                {
                    await next();
                    return;
                }

                try
                {
                    await context.RequestServices
                        .GetRequiredService<IAntiforgery>()
                        .ValidateRequestAsync(context);
                }
                catch (AntiforgeryValidationException exception)
                {
                    app.Logger.LogWarning(
                        exception,
                        "Falha na validação CSRF para {Method} {Path}",
                        method,
                        context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Title = "A validação de segurança da requisição falhou.",
                        Status = StatusCodes.Status400BadRequest
                    });
                    return;
                }

                await next();
            });

            var authApi = app.MapGroup("/api/auth")
                .RequireRateLimiting("AuthRateLimit");

            authApi.MapIdentityApi<AurumUser>();

            // Mapped outside the strict AuthRateLimit group: this endpoint doesn't
            // accept credentials and is called often during normal app usage
            // (page loads, CSRF retries), so it uses the lenient SessionRateLimit.
            app.MapGet("/api/auth/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Headers.CacheControl = "no-store";
                return Results.Ok(new { requestToken = tokens.RequestToken });
            })
                .AllowAnonymous()
                .RequireRateLimiting("SessionRateLimit");

            app.MapControllers(); // Map controller endpoints

            app.Run();
        }
    }
}
