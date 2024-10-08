using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minimal_API.Dominio.ModelViews
{
    public class AdministradorLogado
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}