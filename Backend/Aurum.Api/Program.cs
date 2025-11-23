
using Aurum.API.Extensions;

namespace Aurum.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.AddScooped()
                .AddArchitecture();

            var app = builder.Build();
            app.UseArchitecture()
                .Run();
        }
    }
}
