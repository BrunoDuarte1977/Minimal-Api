using Minimal_API.Dominio.Interfaces;
using Minimal_API.Dominio.DTO;
using Minimal_API.Dominio.Entidades;
using Minimal_API.Infraestrutura.DB;

namespace Minimal_API.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;
        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores.Where(a=> a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        }

        public Administrador Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
            return administrador;
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
             var query = _contexto.Administradores.AsQueryable();
            int itensPorPagina = 10;

            if(pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }
            return query.ToList();
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(x => x.Id == id).FirstOrDefault();
        }
    }
}