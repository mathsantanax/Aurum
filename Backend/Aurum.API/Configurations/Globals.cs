using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aurum.API.Configurations
{
    public class Globals
    {
        public static string DATABASE_URL { get; private set; } = Environment.GetEnvironmentVariable("DATABASE_URL")!;
        public static string  URL_PROJECT { get; private set; } = Environment.GetEnvironmentVariable("URL_PROJECT")!;
        public static string ANON_KEY { get; private set; } = Environment.GetEnvironmentVariable("ANON_KEY")!;
    }
}
