using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace Aurum.Api.Extensions
{
    public static class AppExtension
    {
        public static WebApplication UseArchitecture(this WebApplication app)
        {
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();

            return app;
        } 
    }
}
