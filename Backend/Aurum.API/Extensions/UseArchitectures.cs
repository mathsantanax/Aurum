using Aurum.API.Middlewares;

namespace Aurum.API.Extensions
{
    public static class UseArchitectures
    {
        // Extension method to configure architecture-specific middleware in the WebApplication
        public static WebApplication UseArchitecture(this WebApplication app)
        {
            // Configure architecture-specific middleware here
            // e.g., app.UseMiddleware<MyCustomMiddleware>();

            // Adiciona o middleware de tratamento de exceções personalizado
            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            return app;
        }
    }
}
