using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Dominio.Servicos;
using Minimal_API.Infraestrutura.DB;

namespace Test.Servicos
{
 [TestClass]
    public class AdministradorServicoTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            //Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores"); 

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "adm@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(context);

            //Act
            administradorServico.Incluir(adm);

            //Assert
            Assert.AreEqual(1, administradorServico.Todos(1).Count());
        }

        [TestMethod]
        public void TestarBuscaPorId()
        {
            //Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores"); 

            var adm = new Administrador();
            adm.Id = 1;
            adm.Email = "adm@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "Adm";

            var administradorServico = new AdministradorServico(context);

            //Act
            administradorServico.Incluir(adm);
            var admBanco = administradorServico.BuscaPorId(adm.Id);

            //Assert
            //Assert.AreEqual(1, administradorServico.Todos(1).Count());
            Assert.AreEqual(1, admBanco?.Id);
        }
    }
}