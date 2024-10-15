using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    public class AdministradorRequestTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

         public async Task TestarGetSetPropriedades()
        {
            //Arrange
            var adm = new Administrador();

            var loginDTO = new LoginDTO{
                Email = "adm@teste.com",
                Senha = "123456"   
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            //Act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Email ?? "");
            Assert.IsNotNull(admLogado?.Perfil ?? "");
            Assert.IsNotNull(admLogado?.Token ?? "");
        }
    }
}