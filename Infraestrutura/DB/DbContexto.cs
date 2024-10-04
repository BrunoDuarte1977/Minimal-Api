using Microsoft.EntityFrameworkCore;
using Minimal_API.Dominio.Entidades;

namespace Minimal_API.Infraestrutura.DB
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;

        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }
        public DbSet<Administrador> Administradores { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
                if(!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(
                                            stringConexao, 
                                            ServerVersion.AutoDetect(stringConexao)
                    );
                }
            }
        }
    }
}