using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Aurum.Api.Extensions;

namespace Aurum.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddArchitecture().UseScoped();

            var app = builder.Build();
            app.UseArchitecture();
            app.Run();
        }
    }
}
