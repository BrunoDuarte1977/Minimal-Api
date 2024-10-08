using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;

namespace Minimal_API.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        List<Administrador> Todos(int? pagina = 1);
        Administrador Incluir(Administrador administrador);

        Administrador? BuscaPorId(int id);
    }
}