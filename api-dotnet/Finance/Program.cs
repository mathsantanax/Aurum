using Finance.Extensions;

namespace Finance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder
                .AddArchitecture()
                .AddScoped();

            var app = builder.Build();
            app
                .UseApp()
                .Run();
        }
    }
}
