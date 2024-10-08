using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;

namespace Minimal_API.Dominio.Interfaces
{
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaPorId(int id);

        void Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void Apagar(Veiculo veiculo);
    }
}