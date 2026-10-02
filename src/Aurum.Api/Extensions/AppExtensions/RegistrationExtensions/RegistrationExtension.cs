namespace Aurum.Api.Extensions.AppExtensions.RegistrationExtensions
{
    public static class RegistrationExtension
    {
        public static WebApplication UseRegistrationGuard(this WebApplication app)
        {

            app.Use(async (context, next) =>
            {

                if (context.Request.Method == "POST" &&
                context.Request.Path.StartsWithSegments("/api/auth/register"))
                {
                    var config = context.RequestServices.GetRequiredService<IConfiguration>();
                    var isRegistrationAllowed = config.GetValue<bool>("Registration:AllowRegistration");
                    if (!isRegistrationAllowed)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Cadastro desativado no momento");
                        return;
                    }
                }
                await next();
            });
            return app;
        }
    }
}
