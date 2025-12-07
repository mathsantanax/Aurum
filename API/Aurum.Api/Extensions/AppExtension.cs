using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace Aurum.Api.Extensions
{
    public static class AppExtension
    {
        public static WebApplication UseArchitecture(this WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        } 
    }
}
