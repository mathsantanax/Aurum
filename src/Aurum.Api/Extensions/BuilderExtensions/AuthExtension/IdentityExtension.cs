using Aurum.Infrastructure.Identity;
using Aurum.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Aurum.Api.Extensions.BuilderExtensions.AuthExtension
{
    public static class IdentityExtension
    {
        public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
        {
            var secureCookies = !builder.Environment.IsDevelopment();

            builder.Services.AddIdentityApiEndpoints<AurumUser>(options =>
            {
                // SignIn settings
                options.SignIn.RequireConfirmedEmail = true; // Exige que o usuário confirme o e-mail antes de fazer login
                options.SignIn.RequireConfirmedPhoneNumber = false;// Não exige que o usuário confirme o número de telefone antes de fazer login
                options.SignIn.RequireConfirmedAccount = true; // Exige que o usuário confirme a conta antes de fazer login

                // User settings
                options.User.RequireUniqueEmail = true; // Exige que o e-mail seja único para cada usuário
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";// Define os caracteres permitidos para o nome de usuário

                // Password settings
                options.Password.RequireDigit = true; // Exige que a senha contenha pelo menos um dígito
                options.Password.RequireNonAlphanumeric = true;// Exige que a senha contenha pelo menos um caractere não alfanumérico
                options.Password.RequireUppercase = true;// Exige que a senha contenha pelo menos uma letra maiúscula
                options.Password.RequireLowercase = true; // Exige que a senha contenha pelo menos uma letra minúscula
                options.Password.RequiredLength = 8; // Exige que a senha tenha pelo menos 8 caracteres
                options.Password.RequiredUniqueChars = 1; // Exige que a senha contenha pelo menos 1 caractere único

                // Stores settings
                options.Stores.MaxLengthForKeys = 128; // Define o comprimento máximo para as chaves de armazenamento

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Define o tempo padrão de bloqueio para 5 minutos
                options.Lockout.MaxFailedAccessAttempts = 5; // Define o número máximo de tentativas de acesso com falha antes do bloqueio
                options.Lockout.AllowedForNewUsers = true; // Define se o bloqueio é permitido para novos usuários
            })
                .AddRoles<AurumRoles>()
                .AddEntityFrameworkStores<AurumDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = secureCookies ? "__Host-Aurum.Auth" : "Aurum.Auth";

                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = secureCookies
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.Cookie.Path = "/";
                options.Cookie.Domain = null;

                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                options.LoginPath = "/Login";
                options.AccessDeniedPath = "/access-denied";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PlatformAdminPolicy", policy =>
                {
                    policy.RequireRole("Admin");
                });
                options.AddPolicy("PlatformUsersPolicy", policy =>
                {
                    policy.RequireRole("User");
                });
            });

            var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"]
                ?? Path.Combine(builder.Environment.ContentRootPath, "keys");
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

            return builder;
        }
    }
}
