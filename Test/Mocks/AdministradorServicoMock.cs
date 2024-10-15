using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Dominio.Interfaces;

namespace Test.Mocks
{
    public class AdministradorServicoMock : IAdministradorServico
    {
        private static List<Administrador> administradores = new List<Administrador>(){
            new Administrador{
                Id = 1,
                Email = "adm@teste.com",
                Senha = "123456",
                Perfil = "Adm"
            },
            new Administrador{
                Id = 2,
                Email = "editor@teste.com",
                Senha = "654321",
                Perfil = "Editor"
            }
        };

        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(x => x.Id == id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count() + 1;
            administradores.Add(administrador);
            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return administradores.Find(x => x.Email == loginDTO.Email && x.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
            return administradores;
        }
    }
}