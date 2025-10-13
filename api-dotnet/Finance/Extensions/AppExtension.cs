namespace Finance.Extensions
{
    public static class AppExtension
    {
        public static WebApplication UseApp(this WebApplication app)
        {
            app.UseHttpsRedirection()
                .UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
