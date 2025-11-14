namespace Aurum.Api.Configuration
{
    public static class Globals
    {
        public static string JWT_TOKEN { get; private set; } = Environment.GetEnvironmentVariable("KEY")!;
        public static string JWT_ISSUER { get; private set; } = Environment.GetEnvironmentVariable("ISSUER")!;
        public static string JWT_AUDIENCE { get; private set; } = Environment.GetEnvironmentVariable("AUDIENCE")!;
        public static int EXPIRE_IN_MINUTES { get; private set; } = Convert.ToInt32(Environment.GetEnvironmentVariable("EXPIREINMINUTES"))!;
    }
}
