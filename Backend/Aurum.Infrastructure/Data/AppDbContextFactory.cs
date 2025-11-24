using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("=== [DesignTime] AppDbContextFactory ===");

            // Caminho REAL da pasta Infrastructure (sobe até o csproj)
            var infraPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "../Aurum.Infrastructure")
            );

            Console.WriteLine($"[DesignTime] Infra Path: {infraPath}");

            var envPath = Path.Combine(infraPath, ".env");

            if (!File.Exists(envPath))
            {
                Console.WriteLine($"[DesignTime] .env NÃO encontrado em: {envPath}");
                throw new FileNotFoundException("Arquivo .env não encontrado (Infrastructure)");
            }

            DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envPath }));
            Console.WriteLine($"[DesignTime] .env carregado de: {envPath}");

            var connectionString = Environment.GetEnvironmentVariable("DIRECT_URL");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "❌ Nenhuma connection string encontrada. Defina DIRECT_URL no .env da Infrastructure.");

            Console.WriteLine("[DesignTime] DIRECT_URL carregada com sucesso!");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}
