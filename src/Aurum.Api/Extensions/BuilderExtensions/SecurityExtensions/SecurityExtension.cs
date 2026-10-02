using System.Threading.RateLimiting;

namespace Aurum.Api.Extensions.BuilderExtensions.SecurityExtensions
{
    public static class SecurityExtension
    {
        public static WebApplicationBuilder AddSecurity(this WebApplicationBuilder builder)
        {
            // Add security-related services and configurations here
            // For example, you can add cors, rate limiting, etc.

            var allowedOrigins =
                    builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AurumFrontend", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "__Host-Aurum.Antiforgery";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.Path = "/";
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("AuthRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy("ApiRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                // Used for authenticated/session endpoints (logout, me, csrf token
                // bootstrap) that are not credential-guessing targets but are called
                // frequently during normal app usage (page loads, CSRF retries, etc.).
                options.AddPolicy("SessionRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });
            return builder;
        }
    }
}
