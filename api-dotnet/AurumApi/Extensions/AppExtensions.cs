using AurumApi.Exceptions;

namespace AurumApi.Extensions
{
    public static class AppExtensions
    {
        public static WebApplication UseArchitecture(this WebApplication application)
        {
            application.UseHttpsRedirection();

            application.UseAuthentication();
            application.UseAuthorization();

            application.UseMiddleware<ExceptionMiddleware>();

            application.UseSwagger();
            application.UseSwaggerUI();

            application.MapControllers();


            return application;
        }
    }
}
