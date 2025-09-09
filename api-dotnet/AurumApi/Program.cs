
using AurumApi.Extensions;

namespace AurumApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddArchitecture()
                .AddScopedArchitecture();

            var app = builder.Build();
            app.UseArchitecture()
                .Run();
        }
    }
}
