using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Minimal_API.Infraestrutura.DB
{
    public class DbContextoFactory : IDesignTimeDbContextFactory<DbContexto>
    {
        public DbContexto CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DbContexto>();
            var stringConexao = configuration.GetConnectionString("MySql")?.ToString();
            optionsBuilder.UseMySql(
                stringConexao,
                ServerVersion.AutoDetect(stringConexao)
            );

            return new DbContexto(optionsBuilder.Options, configuration);
        }
    }
}